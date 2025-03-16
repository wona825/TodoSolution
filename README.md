### 사전 요구 사항

- [.NET SDK](https://dotnet.microsoft.com/download) 설치
- [MSSQL](https://www.microsoft.com/ko-kr/sql-server/sql-server-downloads) 설치

### 프로젝트 구조
```
TodoSolution
│
├── Application
│   ├── Contracts
│   ├── DTOs
│   │   ├── Request
│   │   └── Response
│
├── Domain
│   └── Entities
│
├── Infrastructure
│   ├── Data
│   ├── DependencyInjection
│   ├── Error
│   ├── Migrations
│   └── Repo
│
└── WebAPI
    ├── Controllers
    ├── Middleware
    ├── appsettings.json
    └── Program.cs
```

### 프론트엔드 레포
https://github.com/amaran-th/everyone-todo
