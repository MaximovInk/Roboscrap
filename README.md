﻿# SOBERIMUSOR

**Управление:**
- G - Экипировать/Убрать оружие
- R - Перезарядить оружие
- Tab - Открыть/Закрыть оружие
- V - Открыть/Закрыть редактор робота
- E - Взаимодействовать с объектом
- F - Подобрать предмет

**ToDo:**
- [ ] Оружия
- - [ ] Стрелковое оружие
- - - [x] Перезарядка
- - - [ ] ~~Отдача~~
- - - [x] Стрельба очередью
- - - [x] Урон от стрельбы
- - [x] Холодное оружие
- - - [x] Урон

- [x] Инвентарь
- - [x] Поднять/Выбросить
- - [x] Экипировать
- - [x] Крафт

- [x] Мусор
- - [x] Добыча мусора
- - [x] Рандомизация

- [ ] Окружение
- - [ ] Генерация WIP
- - [x] Земля
- - [x] Освещение

**Лог:**


**Version 0.0.8.1**
- Теперь чанки в миникарте так же быстро обновляются , как и позиция игрока

**Version 0.0.8**
- Полностью переделана генерация , теперь границы почти бесконечны , памяти использует мало
- Удален мусор из проекта
- Временно установлено хп для мусора 10
- Удален PathfindingManager
- Добавлена возможность сохранять / загружать сейвы , также там хранится вся информация о чанках
- Дополнены в файл расширений методы для работы с папками и файлами
- Пофикшен краш игры после выхода
- Добавлена базовая миникарта , будет дополняться
- Теперь объекты могут хранить массив данных вместо одного числа
- Добавлен MessagePack для быстрой сериализации данных

**Version 0.0.7**
- Добавлен слайдер для отслеживания генерации мира

**Version 0.0.6**
- Удален LOS Lighting
- Рефакторинг и отчистка кода 
- Локализация
- Меню и пауза
- Загрузочный экран
- Изменен шрифт
- У оружия есть свойство для урона по лому
- Более случайная позиция лома
- Переделан задний фон
- Добавленые новые предметы
- Починен баг с смещением списка компонентов под название рецепта
- Починен баг с несортированными объектами
- Починен интерфейс
- Починены границы чанков
- Починен баг с висящим вверху текстом после исчезновения лома
- Починено хранение информации у лома

**Version 0.0.5**
- Настраивается постепенно генерация
- Добавлены группы рандомных префабов 
- При создании карта посылает префабу -1 вместо 0 (Фикс удаления мусора при старте)
- Изначально теперь тайл всегда будет хранить информацию

**Version 0.0.4**
- Компоненты не имеют прочности
- Созданы префабы пробной окружности
- Небольшие поправки

**Version 0.0.3**
- Оптимизация чанков на основе pool-а

**Version 0.0.2**
- Крафт
- Генерация мусора по шуму перлина
- Система чанков
- Взаимодействие с объектами
- Пули у оружия
- Освещение
- Импортирована tilemap для земли

**Version 0.0.1**
- Ввод
- Передвижение персонажа
- Небольшой набор спрайтов
- Анимации персонажа
- Инвентарь
- Стрельба и удар оружием
- Замена частей робота
- Возможность менять части робота
