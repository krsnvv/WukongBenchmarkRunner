# WukongBenchmarkRunner

Консольная утилита на C# для автоматического запуска **Black Myth: Wukong Benchmark Tool** в двух режимах: с упором на CPU и с упором на GPU.

## Что я сделал

Основная задача была не просто запустить бенчмарк, а пройти весь сценарий без ручного переключения настроек между тестами.

В итоге программа:

1. Находит Steam через реестр Windows.
2. Читает `libraryfolders.vdf` и ищет Benchmark Tool по App ID `3132990`.
3. Находит `GameUserSettings.ini` и делает резервную копию перед первым изменением.
4. Применяет настройки для CPU-теста.
5. Запускает `b1_benchmark.exe`.
6. Находит основной процесс `b1-Win64-Shipping.exe` и его окно.
7. Через Win32 API проходит экран запуска, нажимает `Benchmark` и затем `Confirm`.
8. Ждёт окончания теста.
9. Повторяет тот же сценарий с настройками для GPU-теста.
10. Собирает информацию о CPU, GPU, RAM и Windows.
11. Сохраняет итог в TXT и JSON.

Сторонние NuGet-пакеты не используются. Для работы с окном Benchmark Tool используется P/Invoke и стандартный Win32 API.

## Требования

- Windows 10/11
- Steam
- установленный **Black Myth: Wukong Benchmark Tool**
- .NET 10

Перед запуском программы Benchmark Tool лучше полностью закрыть, если он уже открыт.

## Как запустить

Открыть `WukongBenchmarkRunner.csproj` в Visual Studio и запустить проект через `Ctrl + F5`.

Либо из консоли:

```powershell
dotnet run
```

Путь к Steam или Benchmark Tool указывать вручную не нужно.

## Как подбирались настройки

Настройки Benchmark Tool хранятся в:

```text
b1\Saved\Config\Windows\GameUserSettings.ini
```

Я вручную менял отдельные параметры в интерфейсе Benchmark Tool и сравнивал `GameUserSettings.ini` до и после изменения. Так были получены значения, которые потом используются в программе.

### Preset

```text
Low:
QualityLevel = 1
индивидуальные настройки = 1
ScalabilityGroups = 0

Cinematic:
QualityLevel = 5
индивидуальные настройки = 5
ScalabilityGroups = 4

Custom:
QualityLevel = 6
```

### Frame Generation

```text
InsertFrame = 0 -> Off
InsertFrame = 1 -> On
```

### Ray Tracing

```text
Off:
Rtx = 0
r.RayTracing.EnableInGame = False

On:
Rtx = 1
r.RayTracing.EnableInGame = True
```

Во время проверки отдельно был подтверждён `RtxLevel = 2`, поэтому в GPU-профиле используется именно это значение, без предположений о других уровнях.

## CPU-тест

Профиль CPU сделан так, чтобы максимально снизить влияние видеокарты:

```text
Разрешение:       1280x720
Render Scale:     50%
Preset:           Low
Ray Tracing:      Off
Frame Generation: Off
VSync:            Off
FPS Limit:        Off
```

При Render Scale 50% внутреннее разрешение получается примерно `640x360`. За счёт этого нагрузка на GPU становится ниже, и ограничение со стороны CPU проявляется сильнее.

## GPU-тест

Для GPU-теста используются более тяжёлые настройки:

```text
Разрешение:       1920x1080
Render Scale:     100%
Preset:           Cinematic
Ray Tracing:      On
RtxLevel:         2
Frame Generation: Off
VSync:            Off
FPS Limit:        Off
```

Frame Generation в обоих тестах выключен, чтобы результат отражал реально отрисованные кадры.

## Автоматический запуск теста

Сам `b1_benchmark.exe` запускает основной Unreal-процесс:

```text
b1-Win64-Shipping.exe
```

Главное окно имеет класс `UnrealWindow`. Программа находит его через Win32 API и выполняет необходимые действия мышью относительно размеров окна, поэтому координаты не привязаны к конкретному разрешению рабочего стола.

Последовательность такая:

```text
стартовый экран
    -> главное меню
    -> Benchmark
    -> Confirm
    -> запуск теста
```

## Отчёт

После двух прогонов создаётся папка `Results` рядом с собранным приложением.

В ней появляются два файла:

```text
result-YYYYMMDD-HHMMSS.txt
result-YYYYMMDD-HHMMSS.json
```

TXT нужен для быстрого просмотра, JSON — чтобы результат можно было дальше обрабатывать программно.

## Получение FPS

Benchmark Tool показывает на экране Minimum, Average, Maximum и Low 5% FPS.

`ResultParser` проверяет недавно изменённые `.ini`, `.txt`, `.log`, `.json` и `.csv` файлы внутри `b1\Saved` и пытается найти эти значения там.

На моей проверке точное постоянное место, куда Benchmark Tool сохраняет эти цифры в текстовом виде, однозначно определить не получилось. Поэтому если на конкретной установке значения не сохраняются в читаемый файл, тест всё равно завершится и отчёт будет создан, но поля FPS останутся `N/A`.

Я намеренно не использую `LastCPUBenchmarkResult` и `LastGPUBenchmarkResult` как FPS, потому что нет подтверждения, что эти поля соответствуют цифрам с экрана результата.

## Структура проекта

```text
Interop/     — вызовы Win32 API
Models/      — модели профилей, оборудования и результата
Services/    — поиск Steam, работа с конфигом, запуск и автоматизация Benchmark Tool
Reports/     — сохранение TXT и JSON
Program.cs   — основной сценарий двух последовательных тестов
```
