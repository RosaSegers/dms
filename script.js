import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { uuidv4 } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';

const BASE_URL = 'http://localhost:5093';

export const options = {
  vus: 100,
  stages: [
    { duration: '30s', target: 100 },  // ramp-up to 100 VUs
    { duration: '30s', target: 200 },  // ramp-up to 200 VUs
    { duration: '20m', target: 200 },  // ramp-up to 200 VUs
    { duration: '30s', target: 500 },  // ramp-up to 500 VUs
    { duration: '30s', target: 0 },    // ramp-down to 0
  ],
};

export default function () {
  let documentId;

  group('Upload Document', () => {
    const fileName = `test-${uuidv4()}.txt`;
    const fileContent = 'This is a sample test file for load testing.';
  
    const payload = {
      Name: `Test ${uuidv4()}`,
      Description: 'Load test upload',
      File: http.file(fileContent, fileName, 'text/plain'),
    };
  
    const res = http.post(`${BASE_URL}/api/documents`, payload);
  
    check(res, {
      'upload success': (r) => r.status === 201,
    });
  
    if (res.status === 201) {
      documentId = res.json(); // assuming it just returns the GUID as plain JSON
    } else {
      console.log(`Upload failed: ${res.status} — ${res.body}`);
    }
  });

  sleep(1);

  group('List Documents', () => {
    const res = http.get(`${BASE_URL}/api/documents`);

    check(res, {
      'list success': (r) => r.status === 200,
    });
  });

  sleep(1);

  group('Delete Document', () => {
    if (!documentId) return;

    const res = http.del(`${BASE_URL}/api/documents/${documentId}`);

    check(res, {
      'delete success': (r) => r.status === 204,
    });
  });

  sleep(1);
}