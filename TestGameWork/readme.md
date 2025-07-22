# 🎮 Игра "Крестики-нолики" — Web API

Этот проект представляет собой **веб-API на .NET 9**, реализующий игру в крестики-нолики с поддержкой:
- Создания и присоединения к играм
- Ходов с 10% шансом переворота символа
- JWT-авторизации
- Работы с PostgreSQL через EF Core

Команда запуска юнит-тестирования
```Developer PowerShell
dotnet test TestGameWork.Tests/TestGameWork.Tests.csproj
```
Команда на выполнение docker-compose.yml
```Developer PowerShell
docker-compose up
```
Используется Swager для тестов GameService и Authorization.
[http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html)



---
# 💯 Покрытие кода

В текущей версии покрыто 21% кода. 
Статистика отображена в файле coverage.cobertura.xml, который можно найти в проекте. 
```Команда для анализа текущего покрытия кода
reportgenerator -reports:"TestGameWork.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

--- 

## 🔐 Авторизация

Все эндпоинты защищены с помощью **JWT Bearer**.  
Пользователь должен быть аутентифицирован, чтобы использовать API.

### 🔑 Как получить токен
- При входе/регистрации сервер возвращает JWT-токен
- В дальнейшем токен передаётся в заголовке:

Authorization: Bearer <ваш_токен>

---

## 🧱 Общая архитектура

Авторизация реализована через контроллер `AutorizationController`, который предоставляет эндпоинты:
- `/api/autorization/register` — регистрация нового пользователя
- `/api/autorization/login` — вход и получение JWT-токена

---

## 📦 Используемые компоненты

| Компонент | Назначение |
|---------|-----------|
| `IAutorizationService` | Сервис для бизнес-логики регистрации и входа |
| `RegisterModel` / `LoginModel` | DTO для входных данных |
| `AutorizationResponse` | Ответ с токеном и данными пользователя |
| `ClaimTypes.NameIdentifier` | Идентификатор пользователя в JWT |
| `[Authorize]` | Защита эндпоинтов от неавторизованного доступа |

---

## 🧩 Регистрация (`POST /register`)

### Что происходит:
1. Пользователь отправляет `Name` и `Password`
2. Пароль хешируется (например, с помощью `BCrypt`)
3. Создаётся новый пользователь в БД
4. Генерируется JWT-токен
5. Возвращается `AutorizationResponse` с токеном и данными

---

## 🔐 Вход (`POST /login`)

### Что происходит:
1. Проверяются `Name` и `Password`
2. При успешной проверке:
   - Генерируется JWT-токен
   - В токен добавляются claims:
     - `ClaimTypes.NameIdentifier` — `Guid` пользователя
     - `ClaimTypes.Name` — имя пользователя
3. Возвращается `AutorizationResponse`

---

# 📋 Описание методов контроллера `GameController`

Контроллер `GameController` реализует API для управления играми в крестики-нолики. Все методы защищены атрибутом `[Authorize]` — требуется JWT-токен.

---

## 🔹 `GET Game/list`

**Описание:**  
Возвращает список всех активных игр, в которых участвует текущий пользователь.

**Пример ответа:**
```json
[
  {
    "id": "a1b2c3d4-...",
    "boardSize": 3,
    "board": [["X", null, "O"], [null, "X", null], [null, null, null]],
    "player1": "Alice",
    "player2": "Bob",
    "step": "Bob",
    "status": "Active"
  }
]
```


---

## 🔹 `POST Game/create`

**Описание:**  
Создаёт новую игру. Текущий пользователь становится `Player1`.

**Тело запроса (`CreateGameRequest`):**
```json
{
  "boardSize": 3,
  "winCondition": 3
}
```

**Ответ:**
```json
{
  "id": "a1b2c3d4-...",
  "boardSize": 3,
  "board": [[null,null,null],[null,null,null],[null,null,null]],
  "player1": "Alice",
  "player2": "",
  "step": "Alice",
  "status": "Active"
}
```

---

## 🔹 `GET Game/{gameId}`

**Описание:**  
Присоединяет пользователя к игре или возвращает текущее состояние, если он уже участвует.

**Параметры:**
- `gameId` (в URL) — `Guid` игры

**Пример ответа:**
```json
{
  "id": "a1b2c3d4-...",
  "boardSize": 3,
  "board": [[null,null,null],[null,null,null],[null,null,null]],
  "player1": "Alice",
  "player2": "Bob",
  "step": "Bob",
  "status": "Active"
}
```

---

## 🔹 `PUT Game/{id}/finish`

**Описание:**  
Текущий игрок сдаётся. Статус игры меняется на `Finished`.

**Параметры:**
- `id` (в URL) — `Guid` игры

---

## 🔹 `POST Game/{gameId}/moves`

**Описание:**  
Игрок делает ход. Проверяется:
- Валидность координат
- Очередь игрока
- Возможность победы
- **10% шанс переворота символа** на каждом 3-м ходу

**Параметры:**
- `gameId` (в URL) — `Guid` игры

**Тело запроса (`MakeMoveRequest`):**
```json
{
  "row": 1,
  "column": 1
}
```
