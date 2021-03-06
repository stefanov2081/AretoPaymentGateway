# Areto Payment Gateway
**Areto Payment Gateway** is a payment gateway prototype, developed in the spirit of Domain Driven Design. It aims to demonstrate what a Domain Driven Payment Gateway System might look like. This is a possible way to develop it, but it certainly is not the only way. It is important to note that Domain Driven Design requires deep understanding of the problem domain. Therefore the design in this solution is naive at best. I could have developed a much simpler solution, using EF Entities straight in the API Controller, but I wanted to demonstrate what a decoupled architecture looks like.

There are six projects in the solution
* AretoPaymentGateway.Domain
* AretoPaymentGateway.Application
* AretoPaymentGateway.Infrastructure.Persistence.EfCore
* AretoPaymentGateway.Infrastructure.PaymentAcquirers
* AretoPaymentGateway.Interfaces.WebApi
* AretoPaymentGateway.Interfaces.WebApi.Tests.Integration

The most important projects are AretoPaymentGateway.Domain and AretoPaymentGateway.Application. This is where the business rules are. They are placed in the "center" of the application. Those projects have no external dependencies. On the "periphery" are AretoPaymentGateway.Infrastructure.Persistence.EfCore, AretoPaymentGateway.Infrastructure.PaymentAcquirers and AretoPaymentGateway.Interfaces.WebApi. These projects are secondary. The Domain and Application projects contain abstractions for everything provided by the Infrastructure and Interfaces layers, and it can be substituted in run time.

I have integrated one Payment Provider - Adyen, but this architecture, as naive as it is, allows the integration of many Payment Providers with minimum changes. There are three integration tests in AretoPaymentGateway.Interfaces.WebApi.Tests.Integration that test Authorization, Capture and Cancelling of a Payment. When Authorizing a Payment, Adyen Authorizes it immediately and returns a response to note that. When Capturing a Payment, Adyen returns a response with "received" status. The actual result will be send to a webhook. I have not implemented a web hook for brievety, and I am assuming that the payment is captured sucessfully if the response from Adyen is 200 OK.

# Setup
The solution uses two different databases. A development database for development purposes and a testing database for the integration tests. The integration tests run the migrations automatically. There is no special setup required there. It is enough to build the project and run the tests. When run, the migrations in both projects seed a default user with email: test@test.com and password: qwerty.

To run the project and test the Web Api with a tool like cURL or Postman:
* Open AretoPaymentGateway.sln
* Run Update-Database in package manager console
* Select AretoPaymentGateway.Interfaces.WebApi as startup project
* Start the project with or without debugging (F5 or ctrl + F5)

# Http Request Testing
There are two API Controllers - AuthenticationController and PaymentController. PaymentController requirers authorization using a JSON Web Token. To get a token send a post request to /api/Authenticatoin/login with the user name and the password. The token will be in the response. Copy the value of the token and paste it in an Authorization header with "Bearer " before the token. Example
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cC...

# cURL Example Requests


## /api/Authentication/register
```cURL
curl -X POST \
  https://localhost:44316/api/Authentication/register \
  -H 'Content-Type: application/json' \
  -H 'Postman-Token: 116b5d01-fcba-43a2-bd77-475045daf001' \
  -H 'cache-control: no-cache' \
  -d '{
  "email": "test@test.com",
  "password": "qwerty",
  "confirmPassword": "qwerty"
}'
```

## /api/Authentication/login
```cURL
curl -X POST \
  https://localhost:44316/api/Authentication/login \
  -H 'Content-Type: application/json' \
  -H 'Postman-Token: 1f9938d5-ecda-4873-9b24-9d927a79aea9' \
  -H 'cache-control: no-cache' \
  -d '{
  "username": "test@test.com",
  "password": "qwerty"
}'
```

## /api/Payment/authorize
```cURL
curl -X POST \
  https://localhost:44316/api/Payment/authorize \
  -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoidGVzdEB0ZXN0LmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMSIsImp0aSI6IjZhMGRhNjU4LWRjZmQtNDJjNS05MDI5LWNkYzUwODg2NGVlOSIsImV4cCI6MTY0Nzg5MDE1NSwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NDQzMTYiLCJhdWQiOiIqIn0.XFZlplgJffMP5UXyPI3tF4mlaK2ujowSnp9jZhbNYS8' \
  -H 'Content-Type: application/json' \
  -H 'Postman-Token: 385ee585-b827-4ae4-bd39-880a079ab3e3' \
  -H 'cache-control: no-cache' \
  -d '{
  "amount": 10,
  "currency": "EUR",
  "reference": "Cancel",
  "type": "scheme",
  "cardNumber": "test_4111111111111111",
  "expiryMonth": "test_03",
  "expiryYear": "test_2030",
  "securityCode": "test_737"
}'
```

## /api/Payment/capture
```cURL
curl -X POST \
  https://localhost:44316/api/Payment/capture \
  -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoidGVzdEB0ZXN0LmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMSIsImp0aSI6IjZhMGRhNjU4LWRjZmQtNDJjNS05MDI5LWNkYzUwODg2NGVlOSIsImV4cCI6MTY0Nzg5MDE1NSwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NDQzMTYiLCJhdWQiOiIqIn0.XFZlplgJffMP5UXyPI3tF4mlaK2ujowSnp9jZhbNYS8' \
  -H 'Content-Type: application/json' \
  -H 'Postman-Token: 2bc1f881-c930-44be-b66f-6a184394c3a9' \
  -H 'cache-control: no-cache' \
  -d '{
  "paymentId": 3,
  "amount": 5,
  "currency": "EUR"
}'
```

## /api/Payment/cancel/1
```cURL
curl -X POST \
  https://localhost:44316/api/Payment/cancel/1 \
  -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoidGVzdEB0ZXN0LmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMSIsImp0aSI6IjZhMGRhNjU4LWRjZmQtNDJjNS05MDI5LWNkYzUwODg2NGVlOSIsImV4cCI6MTY0Nzg5MDE1NSwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NDQzMTYiLCJhdWQiOiIqIn0.XFZlplgJffMP5UXyPI3tF4mlaK2ujowSnp9jZhbNYS8' \
  -H 'Content-Type: application/json' \
  -H 'Postman-Token: 1ae1d05f-bc36-48b4-88d9-f2d6fa06921b' \
  -H 'cache-control: no-cache'
```