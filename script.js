import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend } from 'k6/metrics';
import { FormData } from 'https://jslib.k6.io/formdata/0.0.2/index.js';
import { open } from 'k6/experimental/fs';

const loginTrend = new Trend('login_duration');
const profileTrend = new Trend('profile_duration');
const uploadTrend = new Trend('upload_duration');

const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024;
const fileBytes = open('./Code Snippets.docx', 'b');

// Load test options with two scenarios
export const options = {
  scenarios: {
    authScenario: {
      executor: 'constant-vus',
      vus: 50,
      duration: '3m',
      exec: 'authScenario',
    },
    uploadScenario: {
      executor: 'constant-vus',
      vus: 50,
      duration: '3m',
      exec: 'uploadScenario',
      startTime: '30s', // delay upload scenario for better realism
    },
  },
  thresholds: {
    login_duration: ['p(95)<1000'],       // Login must be <1s for 95% of requests
    upload_duration: ['p(95)<2000'],      // Upload must be <2s for 95%
    http_req_failed: ['rate<0.05'],       // Less than 5% failures allowed
  },
  insecureSkipTLSVerify: true,
};

// Reusable login function
function loginAndGetToken() {
  const loginForm = new FormData();
  loginForm.append('Email', 'rosa.segers.2001@gmail.com');
  loginForm.append('Password', 'PasswordPassword');

  const res = http.post('http://131.189.232.222/gateway/auth/login', loginForm.body(), {
    headers: { 'Content-Type': `multipart/form-data; boundary=${loginForm.boundary}` },
  });

  check(res, {
    'login status 200': (r) => r.status === 200,
    'token received': (r) => !!r.json('accessToken'),
  });

  loginTrend.add(res.timings.duration);

  if (res.status !== 200) {
    console.error('Login failed.');
    return null;
  }

  return res.json('accessToken');
}

// Scenario 1: Login + Profile Fetch
export function authScenario() {
  const token = loginAndGetToken();
  if (!token) return;

  sleep(Math.random() * 2 + 1); // simulate user think time

  const profileRes = http.get('http://131.189.232.222/gateway/users/me', {
    headers: { Authorization: `Bearer ${token}` },
  });

  check(profileRes, {
    'profile status 200': (r) => r.status === 200,
  });

  profileTrend.add(profileRes.timings.duration);
  sleep(Math.random() * 2 + 1);
}

// Scenario 2: Login + Upload
export function uploadScenario() {
  const token = loginAndGetToken();
  if (!token) return;

  if (fileBytes.length === 0) {
    console.error('File is empty. Skipping upload.');
    return;
  }

  if (fileBytes.length > MAX_FILE_SIZE_BYTES) {
    console.warn(`File too large: ${fileBytes.length} bytes.`);
    return;
  }

  sleep(Math.random() * 2 + 1);

  const uploadForm = new FormData();
  uploadForm.append('Name', 'Architectural Decisions');
  uploadForm.append('Description', 'Zero downtime soft test');
  uploadForm.append('Version', '1');
  uploadForm.append('File', {
    data: fileBytes,
    filename: 'LoadTest.docx',
    content_type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  });

  const res = http.post('http://131.189.232.222/gateway/documents', uploadForm.body(), {
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': `multipart/form-data; boundary=${uploadForm.boundary}`,
    },
  });

  check(res, {
    'upload status 202': (r) => r.status === 202,
  });

  uploadTrend.add(res.timings.duration);
  sleep(Math.random() * 3 + 2); // longer pause
}
