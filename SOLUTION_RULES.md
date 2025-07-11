# Home Financial — Общие архитектурные правила (aka Clean Architecture)

## Назначение

Это pet-проект, который создается для демонстрации моей технической зрелости и архитектурной компетенции при устройстве на работу.

## 1. Структура проекта

- Domain — бизнес-сущности, value-объекты, интерфейсы для репозиториев/сервисов.
- Application — use case-ы, команды/запросы, DTO, интерфейсы для внешних зависимостей.
- Infrastructure — реализация интерфейсов (репозитории, API, интеграции).
- WebAPI — контроллеры, DI, конфигурация, middleware.

## 2. Зависимости

- Зависимости направлены внутрь: WebAPI → Application → Domain.
- Infrastructure подключается к Application через интерфейсы.

## 3. DTO и маппинг

- DTO создаются в Application, маппятся через Mapster или вручную.
- Внутри Domain — никаких DTO и сторонних библиотек.

## 4. Валидация

- Используется FluentValidation в Application.
- Валидация вызывается вручную в use case.

## 5. Взаимодействие

- Все действия идут через use case (CQRS).
- WebAPI вызывает use case напрямую (без MediatR).

## 6. Интерфейсы

- Интерфейсы репозиториев и внешних сервисов объявляются в Application.
- Реализация — в Infrastructure.

## 7. Особенности проекта

- Предполагается загрузка больших объемов данных (огромные файлы банковских транзакций).
- Ожидается работа с несколькими пользователями одновременно, включая высокую конкурентность операций.
- Все решения должны учитывать производительность, масштабируемость и защиту от race condition.

## 8. Технологии

- .NET 8 — основной фреймворк проекта.
- C# 12 — используемый язык.
- System.Text.Json — сериализация и десериализация JSON.
- FluentValidation — валидация команд и запросов.
- Mapster — маппинг DTO ↔ доменные модели.
- PostgreSQL — основная база данных.
- EF Core — доступ к БД, с реализацией через интерфейсы.
- Docker — контейнеризация окружения.
- Swagger / NSwag — документация и ручной тест API.
- xUnit — модульные тесты.
- Redis — кэширование (опционально).
- ClickHouse + Kafka — для поведенческой аналитики (в перспективе).
- Kubernetes (Minikube, 5 нод) — тестирование масштабирования и устойчивости приложения.
- GitLab CI/CD (локально) — настройка и тестирование примитивного пайплайна.
