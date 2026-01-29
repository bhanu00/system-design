import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

export const options = {
  stages: [
    { duration: '2m', target: 10 }, // Ramp up to 10 users
    { duration: '5m', target: 10 }, // Stay at 10 users
    { duration: '2m', target: 20 }, // Ramp up to 20 users
    { duration: '5m', target: 20 }, // Stay at 20 users
    { duration: '2m', target: 0 },  // Ramp down to 0 users
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
    http_req_failed: ['rate<0.1'],    // Error rate should be less than 10%
    errors: ['rate<0.1'],             // Custom error rate should be less than 10%
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

export default function () {
  // Test GET /api/products
  let response = http.get(`${BASE_URL}/api/products`);
  check(response, {
    'GET /api/products status is 200': (r) => r.status === 200,
    'GET /api/products response time < 500ms': (r) => r.timings.duration < 500,
  }) || errorRate.add(1);

  sleep(1);

  // Test GET /health
  response = http.get(`${BASE_URL}/health`);
  check(response, {
    'GET /health status is 200': (r) => r.status === 200,
    'GET /health response time < 100ms': (r) => r.timings.duration < 100,
  }) || errorRate.add(1);

  sleep(1);

  // Test POST /api/products
  const payload = JSON.stringify({
    name: `Test Product ${Math.random()}`,
    description: 'A test product created during load testing',
    price: Math.floor(Math.random() * 100) + 1,
    category: 'Test'
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  response = http.post(`${BASE_URL}/api/products`, payload, params);
  check(response, {
    'POST /api/products status is 201': (r) => r.status === 201,
    'POST /api/products response time < 1000ms': (r) => r.timings.duration < 1000,
  }) || errorRate.add(1);

  if (response.status === 201) {
    const product = JSON.parse(response.body);
    
    // Test GET /api/products/{id}
    response = http.get(`${BASE_URL}/api/products/${product.id}`);
    check(response, {
      'GET /api/products/{id} status is 200': (r) => r.status === 200,
      'GET /api/products/{id} response time < 300ms': (r) => r.timings.duration < 300,
    }) || errorRate.add(1);

    sleep(1);

    // Test PUT /api/products/{id}
    const updatePayload = JSON.stringify({
      ...product,
      name: `Updated ${product.name}`,
      price: product.price + 10
    });

    response = http.put(`${BASE_URL}/api/products/${product.id}`, updatePayload, params);
    check(response, {
      'PUT /api/products/{id} status is 200': (r) => r.status === 200,
      'PUT /api/products/{id} response time < 500ms': (r) => r.timings.duration < 500,
    }) || errorRate.add(1);

    sleep(1);

    // Test DELETE /api/products/{id}
    response = http.del(`${BASE_URL}/api/products/${product.id}`);
    check(response, {
      'DELETE /api/products/{id} status is 204': (r) => r.status === 204,
      'DELETE /api/products/{id} response time < 300ms': (r) => r.timings.duration < 300,
    }) || errorRate.add(1);
  }

  sleep(2);
}