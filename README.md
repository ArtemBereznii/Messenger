# Messenger API Prototype

A RESTful Web API built with **C#** and **ASP.NET Core**, serving as the backend for a minimal messenger application. This project was built to satisfy the core messaging architecture requirements, along with **Variant 10: Message Moderation and Reporting**.

## 🚀 Features

* **Relational Architecture:** Strict referential integrity between Users, Conversations, and Messages.
* **Offline Support:** A pull-based HTTP architecture allowing offline users to retrieve missed messages upon connection.
* **Advanced Moderation (Variant 10):**
    * Report generation for specific messages.
    * Resolution handling via **Soft Deletion** (maintains an audit trail by flagging `IsHidden`) or **Hard Deletion** ("nuclear" row removal).
* **Automated Database Generation:** Utilizes Entity Framework Core to automatically generate the SQLite database and schema on startup.
* **Integration Testing:** Fully automated xUnit testing pipeline utilizing an in-memory test server (`WebApplicationFactory`).

## 🛠️ Tech Stack

* **Language:** C# 11 / .NET 7 (or 8)
* **Framework:** ASP.NET Core Web API
* **Database:** SQLite (Embedded)
* **ORM:** Entity Framework (EF) Core
* **Testing:** xUnit + `Microsoft.AspNetCore.Mvc.Testing`

## ⚙️ How to Run

Because the application uses an embedded SQLite database and includes an auto-generation script, setup is zero-configuration.

1. Clone the repository.
2. Open a terminal in the `Messenger.Api` folder.
3. Run the application:
   ```bash
   dotnet run
4. Navigate to `http://localhost:<port>/swagger` in your browser to interact with the API visually.

## 📡 API Endpoints

### Users & Conversations
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/users` | Create a new user. Returns a UUID. |
| `POST` | `/conversations` | Initialize a new conversation (direct or group). |
| `GET` | `/conversations/{id}/messages` | Retrieve the chat history (ignores soft-deleted messages). |

### Messaging
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/messages` | Send a message to an existing conversation. |
| `DELETE` | `/messages/{id}` | Permanently remove a message (User Action). |

### Moderation (Variant 10)
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/messages/{id}/report` | Submit a report for a specific message. |
| `POST` | `/reports/{id}/resolve` | Resolve a report with actions: `HIDE`, `DELETE`, or `DISMISS`. |
<<<<<<< HEAD
=======
   dotnet run
>>>>>>> 332da36 (docs: add README)
=======

## 🏗 System Architecture

### Component Diagram

```mermaid
graph TD
    %% Define the client
    Client[REST Client / Postman]

    %% Grouping Controllers
    subgraph "Messenger.Api"
        UsersC[UsersController]
        ConversationsC[ConversationsController]
        MessagesC[MessagesController]
        ModC[ModerationController]
    end

    %% Grouping Data Access & DB
    subgraph "Messenger.Data"
        Context(MessengerContext)
        subgraph "SQLite DB"
            UTable[(Users Table)]
            CTable[(Conversations Table)]
            MTable[(Messages Table)]
            RTable[(Reports Table)]
        end
    end

    %% Client Interactions
    Client ==>|/users| UsersC
    Client ==>|/conversations| ConversationsC
    Client ==>|/messages| MessagesC
    Client ==>|/reports| ModC

    %% Controller Database Operations (Read/Write)
    UsersC -->|Create| UTable
    
    ConversationsC -->|Create| CTable
    ConversationsC -->|Read Messages| MTable
    ConversationsC -.->|Validate IDs| UTable

    MessagesC -->|Send/Hard Delete| MTable
    MessagesC -.->|Validate IDs| UTable
    MessagesC -.->|Validate IDs| CTable

    ModC -->|Create/Read| RTable
    ModC -->|Soft/Hard Delete| MTable

    %% Map entities to Context
    UsersC -.-> Context
    ConversationsC -.-> Context
    MessagesC -.-> Context
    ModC -.-> Context
```

### Sequence diagram of Moderation Workflow

```mermaid
sequenceDiagram
    autonumber
    actor User as Reporter
    actor Admin as Moderator
    participant API as ModerationController
    participant DB as SQLite DB (Reports Table)
    participant MDB as SQLite DB (Messages Table)

    %% Flow A: Report Submission
    Note over User, DB: Flow 1: Submitting a Report
    User->>+API: POST /messages/{msgId}/report {Reason}
    API->>API: Validates Message Existence
    API->>API: Validates Reporter Existence
    API->>DB: Save New Report (IsResolved=False)
    DB-->>API: Confirm ReportID
    API-->>-User: 202 Accepted (Report Submitted)

    %% Flow B: Resolution
    Note over Admin, MDB: Flow 2: Moderator Resolution
    Admin->>+API: POST /reports/{reportId}/resolve {Action}
    API->>API: Verify Action (HIDE, DELETE, DISMISS)
    API->>DB: Read Report Details
    DB-->>API: Return msgId, Reason

    alt Action is HIDE (Soft Delete)
        API->>MDB: Find Message
        MDB-->>API: Message Row
        API->>MDB: UPDATE IsHidden=True, Reason={R}
    else Action is DELETE (Hard Delete)
        API->>MDB: DELETE Row where Id=msgId
    else Action is DISMISS (Audit-Only)
        Note right of API: Message is untouched
    end

    API->>DB: UPDATE IsResolved=True
    API-->>-Admin: 200 OK (Report Resolved)
```

### State Diagram of a Message Life Cycle

```mermaid
stateDiagram-v2
    direction LR
    [*] --> Sent : User sends message
    
    Sent --> Persistent : Saved to SQLite
    
    Persistent --> Reported : User creates report
    Persistent --> Deleted : User hard-deletes
    
    Reported --> Hidden : Admin resolves (Soft Delete)
    Reported --> Persistent : Admin dismisses report
    Reported --> Deleted : Admin resolves (Hard Delete)
    
    Hidden --> [*]
    Deleted --> [*]
    Persistent --> [*] : Read via history
```
>>>>>>> ec65a6f (Revise README with new system architecture diagrams)
