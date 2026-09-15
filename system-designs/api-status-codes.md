Here’s a **complete guide to all HTTP status codes** grouped by category, with meanings, examples, and use cases. HTTP status codes are 3-digit responses sent by servers to indicate the result of a client’s request.

---

## 🔵 1xx – Informational Responses

These indicate that the request was received and understood and that the process is continuing.

| Code                        | Meaning                                           | Description                                  | Use Case                         |
| --------------------------- | ------------------------------------------------- | -------------------------------------------- | -------------------------------- |
| `100 Continue`              | Request headers received                          | Client should continue with the request body | Large POST uploads               |
| `101 Switching Protocols`   | Switching to a new protocol                       | Sent during WebSocket upgrade or HTTP/2      | Upgrading from HTTP to WebSocket |
| `102 Processing` *(WebDAV)* | Server has received and is processing the request | Used when request takes a long time          | Long file operations             |
| `103 Early Hints`           | Hints before the final response                   | Used to preload resources                    | Preloading CSS or JS             |

---

## 🟢 2xx – Success

These indicate the request was successfully received, understood, and accepted.

| Code                                | Meaning                            | Description                       | Use Case                 |
| ----------------------------------- | ---------------------------------- | --------------------------------- | ------------------------ |
| `200 OK`                            | Request succeeded                  | Standard success response         | GET, POST, PUT, DELETE   |
| `201 Created`                       | Resource created                   | New resource created              | POST for user or product |
| `202 Accepted`                      | Request accepted but not completed | Asynchronous processing           | Job queue submission     |
| `203 Non-Authoritative Information` | Modified meta info                 | Returned info may be from a proxy | Response via CDN         |
| `204 No Content`                    | Success but no content             | No body in response               | DELETE or PUT success    |
| `205 Reset Content`                 | Reset the document view            | Tells client to reset form        | After submitting a form  |
| `206 Partial Content`               | Partial content delivered          | Used with range headers           | Download resume support  |

---

## 🟡 3xx – Redirection

These indicate the client must take additional action to complete the request.

| Code                     | Meaning                        | Description                                    | Use Case                    |
| ------------------------ | ------------------------------ | ---------------------------------------------- | --------------------------- |
| `300 Multiple Choices`   | Multiple options               | Returned when more than one resource available | Different language versions |
| `301 Moved Permanently`  | URL changed permanently        | SEO-friendly redirect                          | HTTP to HTTPS               |
| `302 Found`              | Temporary redirect             | URL temporarily at another location            | Post-login redirect         |
| `303 See Other`          | Redirect to GET method         | Used with POST to redirect to a GET            | Redirect after form submit  |
| `304 Not Modified`       | Resource not modified          | Use cached version                             | Conditional GET             |
| `307 Temporary Redirect` | Same method redirect           | Like 302 but method is preserved               | Temporary route changes     |
| `308 Permanent Redirect` | Same method permanent redirect | Like 301 but method preserved                  | API versioning              |

---

## 🔴 4xx – Client Errors

These indicate a problem with the client's request.

| Code                                  | Meaning                                | Description                            | Use Case                        |
| ------------------------------------- | -------------------------------------- | -------------------------------------- | ------------------------------- |
| `400 Bad Request`                     | Malformed request                      | Invalid JSON, query, etc.              | API with incorrect body         |
| `401 Unauthorized`                    | Auth required                          | Missing/invalid credentials            | Protected routes                |
| `402 Payment Required`                | Reserved                               | Rarely used                            | Premium services, Stripe errors |
| `403 Forbidden`                       | Access denied                          | Authenticated but not allowed          | Trying to access admin area     |
| `404 Not Found`                       | Resource not found                     | URL/resource doesn’t exist             | Bad endpoint                    |
| `405 Method Not Allowed`              | Method not allowed                     | E.g., using POST instead of GET        | POST on read-only API           |
| `406 Not Acceptable`                  | Cannot respond with acceptable content | Accept header conflict                 | API doesn’t support XML         |
| `407 Proxy Authentication Required`   | Auth through proxy needed              | Similar to 401, but via proxy          | Enterprise proxy access         |
| `408 Request Timeout`                 | Client took too long                   | Server timed out waiting               | Slow network or uploads         |
| `409 Conflict`                        | Request conflict with current state    | Duplicate username                     | Update on outdated resource     |
| `410 Gone`                            | Resource permanently removed           | Used for deprecated resources          | Deleted blog post               |
| `411 Length Required`                 | Missing Content-Length                 | Server requires length header          | File uploads                    |
| `412 Precondition Failed`             | Conditional request failed             | E.g., If-Match header fails            | Optimistic concurrency          |
| `413 Payload Too Large`               | Request too large                      | Large file or request                  | File upload limit exceeded      |
| `414 URI Too Long`                    | URL too long                           | Excessive query strings                | Bad GET request                 |
| `415 Unsupported Media Type`          | Content-Type not supported             | E.g., text instead of JSON             | API expects JSON                |
| `416 Range Not Satisfiable`           | Range not valid                        | E.g., download range exceeds file size | Resume download beyond size     |
| `417 Expectation Failed`              | Expect header failed                   | Server can’t meet client’s `Expect`    | Rare                            |
| `418 I'm a teapot`                    | Joke status (RFC 2324)                 | Easter egg                             | Not used in production          |
| `422 Unprocessable Entity` *(WebDAV)* | Valid syntax but invalid semantics     | Invalid form field                     | Validation errors in API        |
| `426 Upgrade Required`                | Client must upgrade protocol           | Used in WebSocket                      | Switch to newer TLS version     |
| `429 Too Many Requests`               | Rate limit exceeded                    | API throttling                         | Burst requests from same user   |

---

## 🔴 5xx – Server Errors

These indicate that the server failed to fulfill a valid request.

| Code                             | Meaning                             | Description                     | Use Case                         |
| -------------------------------- | ----------------------------------- | ------------------------------- | -------------------------------- |
| `500 Internal Server Error`      | Generic server error                | Catch-all error                 | Exception thrown                 |
| `501 Not Implemented`            | Feature not implemented             | Server can’t handle the request | PATCH not supported              |
| `502 Bad Gateway`                | Invalid response from upstream      | Reverse proxy or gateway issue  | API Gateway → Lambda fails       |
| `503 Service Unavailable`        | Server is down or overloaded        | Maintenance or overload         | Auto-scaling happening           |
| `504 Gateway Timeout`            | Upstream server timeout             | Long backend process            | DB request hangs                 |
| `505 HTTP Version Not Supported` | Server does not support the version | Very rare                       | HTTP/1.0 deprecated              |
| `507 Insufficient Storage`       | Not enough space to store           | Used by WebDAV servers          | File upload service out of space |

---

## 🔁 C# Example to Handle HTTP Errors in ASP.NET Web API

```csharp
[HttpGet("{id}")]
public IActionResult GetProduct(int id)
{
    var product = _repo.GetById(id);
    if (product == null)
        return NotFound(); // 404

    if (!User.Identity.IsAuthenticated)
        return Unauthorized(); // 401

    return Ok(product); // 200
}
```

---

## ✅ Summary Table

| Category | Range   | Meaning Examples                    |
| -------- | ------- | ----------------------------------- |
| 1xx      | 100–199 | Informational (e.g., 100 Continue)  |
| 2xx      | 200–299 | Success (e.g., 200 OK, 201 Created) |
| 3xx      | 300–399 | Redirection (e.g., 301, 302)        |
| 4xx      | 400–499 | Client Errors (e.g., 400, 404, 403) |
| 5xx      | 500–599 | Server Errors (e.g., 500, 503, 504) |

---

Would you like a downloadable cheat sheet or want to see how to customize error handling globally in .NET Web API?
