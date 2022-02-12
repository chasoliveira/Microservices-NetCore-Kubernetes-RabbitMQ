# Microservices NetCore, Kubernetes, RabbitMQ

Hi, before you keep going further into this project, I would like to let you that I watched this amazing video from 
[Les Jakcson](https://youtu.be/DgVjEo3OGBI)'s channel on Youtube, So I recommend you to go there and leave yours like too.

I created this documentation based on his slide on the video, also the project was created only by watching, but you can find his original project on his [GitHub](https://github.com/binarythistle/S04E03---.NET-Microservices-Course-)

Also take a moment to contribute on his [Patreon Site](https://www.patreon.com/binarythistle)

• Building two .NET Microservices using the REST API pattern
• Working with dedicated persistence layers for both services
• Deploying our services to Kubernetes cluster
• Employing the API Gateway pattern to route to our services
• Building Synchronous messaging between services (HTTP & gRPC)
• Building Asynchronous messaging between services using an Event Bus (RabbitMQ)

## What are Microservices

- Small (2 pizza team, 2 weeks to build)
- Responsible for doing 1 thing well
- Organisationally aligned
- Form part of the (distributed) whole
- Self-contained / Autonomous

### Benefits

- Easier to change & deploy (small and decoupled)
- Can be built using different technologies
- Increased organizational ownership & alignment
- Resilient: 1 service can break, the others will continue to run
- Scalable: You can scale out only the services you need to
- Built to be highly replaceable/swappable

## Solution Architecture

- API Gateway (Nginx)
  - Plataform Service
    - SQl Server
    - REST API
  - Command Service
    - In memory DB
    - REST API
  - RabbitMQ Message Bus (Pub/Sub)
    - Eventual consitency
  - gRPC (Coupling Services)

### Platform Service Architecture

- gRPC Services (Synchronous - in)
- HTTP Request (Synchronous - in)  
  Controllers
    Repository (DTOs <=> Models)
      DBContext (SQL Server DB)
- HTTP Client (Synchronous - out)
- Msg. Publisher (Asyncronous - out) - RabbitMQ Bus

### Command Service Architecture

- Msg. Subscriber (Asyncronous - in) - RabbitMQ Bus
- HTTP Request (Synchronous - in)  
  Controllers
    Repository (DTOs <=> Models)
      DBContext (SQL Server DB)
- gRPC Client (Synchronous - out)

## What is Docker?

It is a `containerization` platform, meaning that it enables you to `package` your applications into `images` and run them as `containers` on any platform that can run Docker.

## Kubernetes

- Built by Google now maintained by the Cloud Native Foundation
- Often referred to as "K8S"
- Container Orchestrator
- Huge subject area!
- 2 broad user profiles
  - Developer
  - Administrator


### Kubernetes Architecture

A Pod is a Container that can be an API, Database (MSSQL), a Message Broker(RabbitMQ, etc)

```
          |------------------------------------------------------------------------------------------------------------------------------|
     |-------------|_____________________________________________________Cluster_________________________________________________________|
     |  Node Port  |______________________________________________________Node__________________________________________________________ |
     |-------------|                                                                                                                   | |
---->| 3xxx <=> 80 |<----|                               ______________________     ______________________     ______________________  | |
     |-------------|     |                               |       Pod          |     |       Pod          |     |       Pod          |  | |
          | |            |                               | |----------------| |     | |----------------| |     | |----------------| |  | |
          | |            |                               | |Platform Service| |     | |  Platform SQL  | |     | |    RabbitMQ    | |  | |
          | |            |                               | |   Container    | |     | |  SQL Container |<|--|  | |    Container   | |  | |
          | |            |                               | |----------------| |     | |----------------| |  |  | |----------------| |  | |
          | |            |-------------------------------|-|-->80  |  666   | |     | | 1433 |           |  |  | | 5672   | 15672 | |  | |
          | |            |                               | |----------------| |     | |------|           |  |  | |----------------| |  | | 
          | |            |                               |--|-------------|---|     |--|------|----------|  |  |--|-------------|---|  | |
          | |            |                                  | 80   |  666 |            | 1433 |             |     | 5672 |15672 |      | |
          | |            |                                  |-------------|            |-------------|      |     |-------------|      | |
          | |            |                                  |  Cluster IP |<-----|---->|  Cluster IP |<---------->|  Cluster IP |      | |
          | |            |                                  |-------------|      |     |-------------|      |     |-------------|      | |
          | |            |                                                       |                          |                          | |
          | |            |------------------------|                              |                          |                          | |
          | |                                     |      ______________________  |                          |                          | |
          | |             |------------------|    |      |       Pod          |  |                          |                          | |
          | |             |       Pod        |    |      | |----------------| |  |                          |                          | |
      |--------------|    | |--------------| |    |      | | Command Service| |  |      |---------------|   |                          | |
      | Ingress Ngix |    | | Ingress Nginx|<|----|      | |   Container    | |  |      |   Persistent  |<--|                          | |
      | Load Balance |    | |  Container   | |    |      | |----------------| |  |      | Volume Claim  |                              | |
      |--------------|    | |--------------| |    |------|-|-->80  |          |  |      |---------------|                              | |
----->|   80   |<---------|>|   80   |       |           | |-------|          |  |                     |                               | |
      |--------|          | |--------|       |           |--|-------------|---|  |                     |                               | |
          | |             |------------------|              | 80   |  666 |      |                     |                               | |
          | |                                               |-------------|      |                     |                               | |
          | |                                               |  Cluster IP |<-----|                     |                               | |
          | |                                               |-------------|                            |                               | |
          | |__________________________________________________________________________________________|_______________________________| |
          |                                                                                            |                                 |
          |-----------------------------------------------------Kubernetes (Docker Desktop)------------|---------------------------------|
          |-------------------------------------------------------Container Runtime (Docker)-----------|---------------------------------|
          |------------------------------------------------Operating System (Windows 10 with WSL2)-----|---------------------------------|
          |-------------------------------------------------------------Hardware (PC)----------------------------------------------------|
          |------------------------------------------------------------------------------------------------------------------------------|
```

## Synchronouns Messaging

- Request/Response Cycle
- The requester will "wait" for a response
- Externally facing services are usually synchronous (e.g. HTTP requests)
- Services usually need to "know" about each other
- We are using 2 forms:
  - Http
  - gRPC

### Wait! What if I mark HTTP actions as Async?

```cs
  [HttpGet]
  public async Task<ActionResult> Get(){

    return Ok(await Task.FromResult("Ssolved!"));
  }
```
- From a messaging perspective this method is still synchronous
- The client still has to wait for a response
- Async in this context (the C# language) means that the `action will not wait for a long-running operation
- It will hand back its thread to the thread pool, where it can be reused
- When the operation finishes it will re-acquire a thread and complete, (and respond back to the requestor)
- So Async here is about thread exhaustion - the requestor still has to wait (the call is asynchronous)

### Synchronous messaging between services

- Can and does occur = we will implement, however...
- it does tend to pair services, (couple them), creating a dependency
- Could lead to long dependency chains

```img
  | Service A |<|----|                           |---|> | Service C |
                     |----|> | Service B | <|----|
                                                 |---|> | Service D | ----|> | Service E |
```

## Asynchronous Messaging

- No Request/Response Cycle
- The requester does not wait
- Event model, e.g. publish/subscribe
- Typically used between services
- Event bus is often used (we'll be using RabbitMQ)
- Services don't need to know about each other, just the bus
- Introduces its own range of complexities - not a magic bullet

### Wait! Isn't the event bus a Monolith?

- To some extent yes
- Internal comms would cease if the message bus goes down
- Should be treated as a first-class citizen, similar to:
  - Network, physical storage, power, etc.
- Message bus should be clustered, with message persistence, etc.
- Services should implement some kind of retry policy
- Aim for Smart Services, stupid pipes.

```
    [Service A]   [Service B]   [Service E]
        |             |             |
  |-----------------------------------------|
  |               Message Bus               |
  |-----------------------------------------|
                |             |
            [Service C]   [Service D]
```


## RabbitMQ

### What is it?

- A Message Broker - it accepts and forwards messages
- Messages are sent by Producers (or Publishers)
- Messages are received by Consumers (or Subscribers)
- Messages are stored on Queues (essentially a message buffer)
- Exchanges can be used to add "routing" functionality
- Uses Advanced Message Queuing Protocol (AMQP) & others

### Exchanges
 Four Types:
  * Direct
    Delivers Messages to queues based on routing key
    Ideal for "direct" or unicast messaging
  * Fanout
    Delivers Messages to all Queues that are bounded to the exchange
    It ignores the routing key
    Ideal for broadcast messages
  * Topic
    Routes messages to 1 or more queues based on the routing key (and patterns))
    Used for multicast messaging
    Implements various Publisher/Subscriber Patterns
  * Header

## gRPC

 - "Google" Remote Procedure Call
 - Uses HTTP/2 protocol to transport binary messages (in. TLS)
 - Focused on high performance
 - Relies on "Protocol Buffers" (aka Protobuf) to defined the contract between endpoints
 - Multi-language support (C# client can call a Ruby Service)

 ```
  |---------------------------|            |-----------------------------|
  |        C# Client          |            |         Ruby Service        |
  | |--------| |-----------|  |            |  |-------------| |--------| |
  | | .proto | | gRPC stub |<-|---HTTP/2---|->| gRPC server | | .proto | |
  | |--------| |-----------|  |            |  |-------------| |--------| |
  |---------------------------|            |-----------------------------|
 ```