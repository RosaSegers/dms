package main

import (
	"context"
	"crypto/tls"
	"encoding/json"
	"fmt"
	"log"
	"os"
	"os/signal"
	"strings"
	"syscall"

	"github.com/go-redis/redis/v8"
	"github.com/rabbitmq/amqp091-go"
)

var (
	redisClient *redis.Client
	ctx         = context.Background()
)

type LogMessage struct {
	UserId      string `json:"UserId"`
	Message     string `json:"Message"`
	RequestName string `json:"RequestName"`
	RequestId   string `json:"RequestId"`
	Severity    string `json:"Severity"`
	Metadata    string `json:"Metadata"`
}

func main() {
	rabbitmqURL := mustReadSecret("/secrets/rabbitmq-url")
	redisAddr := mustReadSecret("/secrets/redis-addr")
	redisPassword := mustReadSecret("/secrets/redis-password")

	// Connect to RabbitMQ
	conn, err := amqp091.Dial(rabbitmqURL)
	if err != nil {
		log.Fatalf("Failed to connect to RabbitMQ: %v", err)
	}
	defer conn.Close()

	ch, err := conn.Channel()
	if err != nil {
		log.Fatalf("Failed to open a channel: %v", err)
	}
	defer ch.Close()

	queueName := "logs"
	_, err = ch.QueueDeclare(
		queueName, true, false, false, false, nil,
	)
	if err != nil {
		log.Fatalf("Failed to declare a queue: %v", err)
	}

	msgs, err := ch.Consume(queueName, "", false, false, false, false, nil)
	if err != nil {
		log.Fatalf("Failed to register a consumer: %v", err)
	}

	redisClient = redis.NewClient(&redis.Options{
		Addr:     redisAddr,
		Password: redisPassword,
		TLSConfig: &tls.Config{
			MinVersion: tls.VersionTLS12,
			ServerName: strings.Split(redisAddr, ":")[0],
		},
	})

	// Test Redis connection
	if err := redisClient.Ping(ctx).Err(); err != nil {
		log.Fatalf("Failed to connect to Redis at %s: %v", redisAddr, err)
	}
	log.Printf("Connected to Redis at %s", redisAddr)

	// Handle graceful shutdown
	sigc := make(chan os.Signal, 1)
	signal.Notify(sigc, syscall.SIGINT, syscall.SIGTERM)

	log.Println("Service started. Waiting for messages...")

	go func() {
		for d := range msgs {
			log.Printf("Received a message: %s", d.Body)

			var logMsg LogMessage
			err := json.Unmarshal(d.Body, &logMsg)
			if err != nil {
				log.Printf("Failed to parse JSON log message: %v", err)
				d.Nack(false, true)
				continue
			}

			log.Printf("Parsed LogMessage: %+v", logMsg)

			err = storeMessage(string(d.Body))
			if err != nil {
				log.Printf("Error storing message in Redis: %v", err)
				d.Nack(false, true)
				continue
			}

			d.Ack(false)
		}
	}()

	<-sigc
	log.Println("Shutting down service...")
}

func storeMessage(msg string) error {
	key := "rabbitmq_messages"
	err := redisClient.RPush(ctx, key, msg).Err()
	if err != nil {
		return fmt.Errorf("failed to push message to Redis list: %w", err)
	}
	return nil
}

func mustReadSecret(path string) string {
	data, err := os.ReadFile(path)
	if err != nil {
		log.Fatalf("Failed to read secret at %s: %v", path, err)
	}
	return strings.TrimSpace(string(data))
}
