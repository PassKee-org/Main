Привет! Как архитектор, я проанализировал задачу и подготовил план реализации базового функционала **Vaults, Directories и Credentials**, учитывая требования к безопасности и текущую архитектуру PassKee (включая ZKP, Drag-and-Drop, Fluxor и т.д.).

### Детали Имплементации (MVP)

#### 1. Структура БД (Entities & Migrations)
Создадим следующие сущности и таблицы:
- **`VaultEntity`**: `Id`, `UserId` (владелец), `Name` (открытый текст).
  *Имена Vaults мы оставляем открытыми, чтобы быстро формировать список Vaults на клиенте до расшифровки основного контента (как просили в задаче - не шифровать всё подряд).*
- **`DirectoryEntity`**: `Id`, `VaultId`, `ParentDirectoryId` (nullable, для иерархии), `EncryptedName` (`byte[]`).
- **`CredentialEntity`**: `Id`, `VaultId`, `DirectoryId` (nullable), `Type` (Enum: Login, SecureNote, Card, и т.д.), `EncryptedBody` (`byte[]`).

#### 2. Шифрование и Session Storage
- У пользователя уже есть `EncryptedUserVaultKey`, зашифрованный `MasterKey` (в `UserEntity`).
- При успешном логине клиентское приложение расшифровывает этот `VaultKey` и сохраняет его в **Session Storage**. Это гарантирует, что ключ не переживет закрытие вкладки (реализация блокировки табы).
- Все операции шифрования `Directory.EncryptedName` и `Credential.EncryptedBody` будут производиться клиентом через `VaultKey` с использованием AES-256-GCM.

#### 3. API & DTOs (`PassKee.Api` и `PassKee.Api.Shared`)
- **GET /api/vaults** — Получить список Vaults пользователя (`Id`, `Name`).
- **GET /api/vaults/{vaultId}** — Единый запрос для получения всех данных Vault:
  - Возвращает детали Vault.
  - Список `DirectoryDto` (`EncryptedName`).
  - Список `CredentialDto` (`Type`, `EncryptedBody`).
*Обрати внимание: сущности возвращаются списком, каждая зашифрована отдельно, а не одним большим блоком.*

**DTO Credentials (Полиморфизм на клиенте)**
Создадим базовую модель `BaseCredentialPayload` и наследников (например, `LoginCredentialPayload`, `NoteCredentialPayload`). На клиенте `EncryptedBody` будет расшифровываться в JSON-строку, а затем маппиться (десериализоваться) в нужную DTO в зависимости от поля `Type`.

#### 4. Архитектура Клиента (`PassKee.Web` + Fluxor)
- **State**: В `VaultState` (или `DataState`) будут храниться *расшифрованные* сущности.
- **Effects**:
  1. Экшен `LoadVaultDataAction`.
  2. Effect делает запрос к API.
  3. Effect берет `VaultKey` из Session Storage.
  4. Проходит по списку директорий и расшифровывает их имена.
  5. Проходит по списку credentials, расшифровывает их тела и маппит в нужные DTO.
  6. Кладет расшифрованные данные во Fluxor In-Memory Storage (`LoadVaultDataSuccessAction`).
- **UI Layout**:
  - На главной странице сверху слева будет AppSelect/Dropdown для выбора текущего Vault.
  - Под ним — иерархический список директорий (дерево) с поддержкой Drag-and-Drop.
  - Справа — список Credentials в выбранной директории и детальная панель (стилизуем стандартными компонентами `PassKee.Web.Core`).

---

**Приступаю к имплементации!**
