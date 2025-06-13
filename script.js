import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend } from 'k6/metrics';
import { FormData } from 'https://jslib.k6.io/formdata/0.0.2/index.js';
import { open } from 'k6/experimental/fs';

// Metrics
let loginTrend = new Trend('login_duration');
let profileTrend = new Trend('profile_duration');
let uploadTrend = new Trend('upload_duration');

// Read file as binary Uint8Array directly (no base64 decode)
const fileBytes = open('./LoadTest.docx', 'b');

export let options = {
  vus: 50,
  duration: '5m',
  thresholds: {
    login_duration: ['p(95)<1000'],
    upload_duration: ['p(95)<2000'],
    http_req_duration: ['p(95)<3000'],
  },
  insecureSkipTLSVerify: true,
};

export default function () {
  // LOGIN multipart/form-data
  const loginForm = new FormData();
  loginForm.append('Email', 'rosa.segers.2001@gmail.com');
  loginForm.append('Password', 'PasswordPassword');

  const loginRes = http.post('https://dmsgateway.local/gateway/auth/login', loginForm.body(), {
    headers: { 'Content-Type': `multipart/form-data; boundary=${loginForm.boundary}` },
    tags: { endpoint: '/auth/login' },
  });

  check(loginRes, {
    'login status 200': (r) => r.status === 200,
    'login has token': (r) => !!r.json('accessToken'),
  });

  loginTrend.add(loginRes.timings.duration);

  if (loginRes.status !== 200) {
    console.error('Login failed, stopping iteration.');
    return;
  }

  const token = loginRes.json('accessToken');
  sleep(Math.random() * 2 + 1);

  // VIEW PROFILE (simulate some post-login usage)
  const profileRes = http.get('https://dmsgateway.local/gateway/users/me', {
    headers: { Authorization: `Bearer ${token}` },
    tags: { endpoint: '/users/me' },
  });

  check(profileRes, {
    'profile status 200': (r) => r.status === 200,
  });

  profileTrend.add(profileRes.timings.duration);
  sleep(Math.random() * 2 + 1);

  // UPLOAD DOCUMENT multipart/form-data
  const uploadForm = new FormData();
  uploadForm.append('Name', 'Architectural Decisions');
  uploadForm.append('Description', 'This document contains important architectural decisions.');
  uploadForm.append('Version', '1');
  uploadForm.append('File', {
    data: fileBytes,
    filename: 'LoadTest.docx',
    content_type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  });

  const uploadRes = http.post('https://dmsgateway.local/gateway/documents', uploadForm.body(), {
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': `multipart/form-data; boundary=${uploadForm.boundary}`,
    },
    tags: { endpoint: '/document' },
  });

  // Add logging here for debugging upload issues


  check(uploadRes, {
    'upload status 202': (r) => r.status === 202,
  });

  uploadTrend.add(uploadRes.timings.duration);
  sleep(Math.random() * 3 + 2);
}
