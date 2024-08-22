# WebAPI 배포 가이드

이 가이드는 `dotnet` 명령어를 사용하여 WebAPI 서버를 배포하는 방법을 설명합니다.

## 사전 요구 사항

- [.NET SDK](https://dotnet.microsoft.com/download) 설치
- [MSSQL](https://www.microsoft.com/ko-kr/sql-server/sql-server-downloads) 설치

## 프로젝트 구조
```
TodoSolution
│
├── Application
│   ├── Contracts
│   │   └── IAuth.cs
│   ├── DTOs
│   │   ├── Request
│   │   │   ├── LoginRequest.cs
│   │   │   ├── RefreshTokenRequest.cs
│   │   │   └── RegisterUserRequest.cs
│   │   └── Response
│   │       ├── LoginResponse.cs
│   │       ├── RefreshTokenResponse.cs
│   │       └── RegisterUserResponse.cs
│   └── Application.csproj
│
├── Domain
│   ├── Entities
│   │   ├── ApplicationUser.cs
│   │   └── Token.cs
│   └── Domain.csproj
│
├── Infrastructure
│   ├── Data
│   │   └── AppDbContext.cs
│   ├── DependencyInjection
│   │   └── ServiceContainer.cs
│   ├── Error
│   │   └── CustomException.cs
│   ├── Migrations
│   │   ├── 20240821194219_First.cs
│   │   ├── 20240821194219_First.Designer.cs
│   │   └── AppDbContextModelSnapshot.cs
│   ├── Repo
│   │   └── AuthRepo.cs
│   └── Infrastructure.csproj
│
└── WebAPI
    ├── Controllers
    │   ├── AuthController.cs
    │   └── WeatherForecastController.cs
    ├── Middleware
    │   ├── ExceptionHandlingMiddleware.cs
    │   └── JwtMiddleware.cs
    ├── appsettings.json
    ├── Program.cs
    └── WebAPI.csproj

```

## 1. 의존성 복원

먼저, 프로젝트의 모든 의존성을 복원합니다. 솔루션 디렉토리에서 다음 명령어를 실행하세요: `dotnet restore`
## 2. 빌드

프로젝트를 빌드합니다. 솔루션 디렉토리에서 다음 명령어를 실행하세요: `dotnet build`
## 3. 데이터베이스 마이그레이션

만약 데이터베이스 마이그레이션이 필요하다면, `dotnet-ef`를 설치하기 위해 다음 명령어를 실행하세요: `dotnet tool install --global dotnet-ef`
이후에 `WebAPI` 프로젝트 디렉토리에서 다음 명령어를 실행하세요: `dotnet ef database update`
## 4. 서버 실행

서버를 실행합니다. `WebAPI` 프로젝트 디렉토리에서 다음 명령어를 실행하세요: `dotnet run --launch-profile https`
서버가 성공적으로 시작되면, 기본적으로 `https://localhost:7133`에서 API에 접근할 수 있습니다.

`https://localhost:7133/swagger/index.html`를 통해서 swagger를 실행해 관련 정보를 확인할 수 있습니다.

## 5. 배포

배포를 위해 프로젝트를 게시합니다. `WebAPI` 프로젝트 디렉토리에서 다음 명령어를 실행하세요: `dotnet publish -c Release -o ./publish`
이 명령어는 `Release` 구성으로 프로젝트를 빌드하고, `./publish` 디렉토리에 결과물을 출력합니다. 이 디렉토리를 서버에 배포할 수 있습니다.

## 6. 서버에서 실행

서버에서 실행하려면, `WebAPI` 프로젝트 디렉토리에서 다음 명령어를 실행하세요: `dotnet ./publish/WebAPI.dll`
이제 서버가 실행되고, API에 접근할 수 있습니다. (포트가 변경될 수 있으니 터미널을 확인해주세요.)

## 추가 정보

- [ASP.NET Core 문서](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core 문서](https://docs.microsoft.com/ef/core)
