<h1 align="center" id="title">xbot</h1>



Технологии используемые в этом проекте:

*   C#
*   Entity framework
*   Sqlite

Бот написан под приватный сервер, и не спроектирован под массовое использование.
Данный бот рассчитан на изолированное использование, которое подразумевает, выдачу доступа к определенным функциям по ролям в боте. 
Данный бот имеет 3 категории, у каждого доступ только к своим функциям:

*   Админ - имеет доступ к варнам, и удалению варнов. Может проводить и отменять ивенты
*   Модератор - Имеет доступ только к варнам, он не может убрать варн.
*   Ивентер - Имеет доступ к розыгрышам.

<h2>🛠️ Installation Steps:</h2>

1.Скопируйте и откройте проект<br>
2.Дождитесь окончание подгрузки библиотек<br>
3.Запустите проект и получите ошибку!<br>
4.Скопируйте корневой файл _config.yml в bin/Debug/.net7.0<br>
5.Вставьте ваш токен вместо "your-token" (https://discordgid.ru/token/)<br>
6.Запустите проект<br>

# 🍰 Возможности

Бот предоставляет разнообразные функции для улучшения опыта пользователей на сервере. Ниже представлен краткий обзор ключевого функционала:

1. **Логирование действий пользователя**
   - Вход и выход пользователя с сервера
   - Бан и кик пользователя с сервера
   - Изменение и удаление сообщений пользователя
   - Mute, Unmute, Defear, UnDefear, Stream, Stream end
   - Информация о том, от какого пользовательского инвайта пришел пользователь

2. **Розыгрыш призов**

3. **Создание приватных каналов**

4. **Гибкая система Warns**
   - Логирование действий модерации в отношении варнов
   - Информация о времени выдачи варна, причине, модераторе, и снятии варна

5. **Система ролей**
   - Уровневые роли за общение в чате
   - Репутационные роли за определенное количество репутаций
   - Покупные роли за валюту бота
   - Роли за приглашения (с ограничением по количеству активных пользователей)

6. **Система приглашений**
   - Мотивация пользователей приглашать людей на сервер
   - Роли за количество приглашений, с увеличением роли по мере роста числа приглашений
   - Защитные функции для предотвращения накрутки приглашений

7. **Мелкие функции**
   - Свадьба с другим пользователем (шуточная sex функция)
   - Поздравление с днем рождения
   - Roleplay гифки
   - Совместимость человека с вами
   - Ежедневное получение вознаграждения (Daily)
   - Ставки виртуальной валюты в казино (Kazino)
   - Топ пользователей по опыту, деньгам и репутациям

Это только часть функционала бота, который также включает в себя множество других мелких решений.
