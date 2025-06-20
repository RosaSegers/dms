import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend } from 'k6/metrics';
import { FormData } from 'https://jslib.k6.io/formdata/0.0.2/index.js';
import { open } from 'k6/experimental/fs';

let loginTrend = new Trend('login_duration');
let profileTrend = new Trend('profile_duration');
let uploadTrend = new Trend('upload_duration');

const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024;
const fileBytes = open('./Code Snippets.docx', 'b');

export let options = {
  vus: 5,  // only 3 concurrent users
  duration: '5m',  
  thresholds: {
    http_req_failed: ['rate<0.05'],  // allow up to 5% failure
  },
  insecureSkipTLSVerify: true,
};

export default function () {
  const loginForm = new FormData();
  loginForm.append('Email', 'rosa.segers.2001@gmail.com');
  loginForm.append('Password', 'PasswordPassword');

  const loginRes = http.post('http://131.189.232.222/gateway/auth/login', loginForm.body(), {
    headers: { 'Content-Type': `multipart/form-data; boundary=${loginForm.boundary}` },
  });

  check(loginRes, {
    'login status 200': (r) => r.status === 200,
    'login has token': (r) => !!r.json('accessToken'),
  });

  loginTrend.add(loginRes.timings.duration);
  if (loginRes.status !== 200) {
    console.error('Login failed, skipping iteration.');
    return;
  }

  const token = loginRes.json('accessToken');
  sleep(2);

  const profileRes = http.get('http://131.189.232.222/gateway/users/me', {
    headers: { Authorization: `Bearer ${token}` },
  });

  check(profileRes, {
    'profile status 200': (r) => r.status === 200,
  });

  profileTrend.add(profileRes.timings.duration);
  sleep(2);

  if (fileBytes.length === 0) {
    console.error('File is empty. Skipping upload.');
    return;
  }

  if (fileBytes.length > MAX_FILE_SIZE_BYTES) {
    console.warn(`File too large: ${fileBytes.length} bytes.`);
    return;
  }

  const uploadForm = new FormData();
  uploadForm.append('Name', 'Architectural Decisions');
  uploadForm.append('Description', 'Zero downtime soft test');
  uploadForm.append('Version', '1');
  uploadForm.append('File', {
    data: fileBytes,
    filename: 'LoadTest.docx',
    content_type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  });

  const uploadRes = http.post('http://131.189.232.222/gateway/documents', uploadForm.body(), {
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': `multipart/form-data; boundary=${uploadForm.boundary}`,
    },
  });

  check(uploadRes, {
    'upload status 202': (r) => r.status === 202,
  });

  uploadTrend.add(uploadRes.timings.duration);
  sleep(3);  // longer pause to be gentle
}
