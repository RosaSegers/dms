# Build stage
FROM golang:1.24-alpine AS builder

WORKDIR /app

COPY go.mod go.sum ./
RUN go mod download

COPY . .

RUN go build -o logconsumer .

# Final stage
FROM alpine:latest

WORKDIR /app

COPY --from=builder /app/logconsumer .

# Use environment variables for configuration
ENV RABBITMQ_URL=amqp://guest:guest@rabbitmq:5672/
ENV REDIS_ADDR=redis:6379

CMD ["./logconsumer"]
