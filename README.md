# xbot

Дискорд бот написанный на C# с использованием Entity framework и sqlite.
Бот написан под приватный сервер, и не спроектирован под массовое использование.
Данный бот рассчитан на изолированное использование, которое подразумевает, выдачу доступа к определенным функциям по ролям в боте. 
Данный бот имеет 3 категории Админы, модераторы, и ивенторы, у каждого доступ только к своим функциям.

Админ - имеет доступ к варнам, и удалению варнов. Может проводить и отменять ивенты
Модератор - Имеет доступ только к варнам, он не может убрать варн.
Ивентер - Имеет доступ к розыгрышам.

## Установка

1.Скопируйте и откройте проект
2.Дождитесь окончание подругзки библиотек
3.Запустите проект и получите ошибку!
4.Скопируйте корневой файл _config.yml в bin/Debug/.net7.0
5.Вставьте ваш токен вместо "your-token" (https://discordgid.ru/token/)
6.Запустите проект

### Возможности

1.Логирование действий пользователя
  1)Вход, Выход пользователя с сервера
  2)Бан, кик пользователя с сервера
  3)Изменение, Удаление сообщений пользователя
  5)Mute, Unmute, Defear, UnDefear, Stream, Stream end
  6)Информация о том от какого пользовательского инвайта пришел пользователь

2.Розыгрыш призов
3.Создание приватных каналов
4.Гибкая система Warns, которая логирует действия модерации в отношении варнов, можно узнать информацию когда был варн, какая причина, кто выдал, и кто убрал.

5.Система ролей
  1)Уровневые роли, которые выдаются за общение в чате
  2)Репутационные роли, выдаютя за определенное количество репутаций, выдаваемых пользователями
  3)Покупные роли. Роли которые можно купить за валюту бота.
  4)Роли за приглашения. О них ниже...

6.Система приглашений.
Система которая позволяет мотивировать пользователей приглашать людей на сервер. За приглашение человека, пользователь получает роль, чем больше приглашений, тем выше роль.
Данная система так же имеет защитные функции, дабы пользователь не накрутил приглашения. Роли помимо количества приглашений, имеют ограничение в количестве активных пользователей в неделю и пользователей которые достигли 5 уровня.

7.Мелкие функции:
1)Свадьба с другим пользователем (шуточкая sex функция)
2)Поздравление с днем рождения
3)Roleplay гифки
4)Совместимость человека с вами
5)Daily - получение ежедневного вознаграждения
6)Kazino - ставки виртуальной валюты бот
7)Топ пользователей (По опыту, по денегам, по репутациям)
