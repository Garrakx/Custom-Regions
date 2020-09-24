
# [BETA] Custom-Regions

## Позволяет устанавливать новые регионы, не редактируя базовые файлы игры. Автоматически совмещает файлы мира и перенаправляет соединения между комнатами.

#### # *ДИСКЛЕЙМЕР* Данный мод находится в разработке. Будьте готовы к багам! 
![Custom Regions!](http://www.raindb.net/previews/customregion.png?raw=true)
### Оглавление
* [Установка мода](#index1)
* [Установка новых регионов](#index2)
* [Удаление](#index3)
* [Как это работает?](#index4)
* [Обработка конфликтов - совмещение регионов](#index5)
* [Information for Modders](#index6)
* [Known issues](#index7)
* [Credits](#index8)

### <a name="index1"></a>Установка мода
1) Скачайте и установите последнюю версию Partiality Launcher из [раздела "Инструменты" RainDB's](http://www.raindb.net/)
2) Скачайте последнюю версию CR [отсюда](https://github.com/Garrakx/Custom-Regions/releases/)
3) Применить **ВСЕ** (`EnumExtender.dll, AutoUpdate.dll, CustomAssets.dll и CustomRegions.dll`) моды из архива `[DOWNLOAD_THIS_Custom-Regions-vX.X.zip]`. Обновления будут устанавливаться автоматически.

### <a name="index2"></a>Установка новых регионов
* ***Внимание:** Большая часть инструкций, поставляемых с файлами регионов, устарела. Для использования данного мода следуйте дальнейшим указаниям из этого файла.*
1) Внутри Rain World\Mods создайте новую папку "`CustomResources`" (*iзапуск игры со включенным модом создаст её автоматически*)
2) Создайте новую папку в Rain World\Mods\CustomResources под названием, соответствующем интересующему вас региону (`пример: Rain World\Mods\CustomResources\The Root`). Это определит имя региона в игре.
3) Внутрь этой директории региона поместите папки "`World`", "`Assets`" и / или "`Levels`" из архива, в котором регион поставляется.
4) После запуска игры мод создаст файл под названием "regionInfo.json". Этот файл может быть открыт любым текстовым редактором (например, notepad). Убедитесь, что вся информация выглядит правильно. К каждому региону можно добавить описание. Порядок расположения показывает, какие регионы будут загружаться раньше, а какие позже. Если ваша версия использовала regionID.txt, мод произведёт попытку апгрейда. Чтобы применить изменения, необходимо перезапустить игру и **НАЧАТЬ ИГРУ НА ЧИСТОЙ ЯЧЕЙКЕ СОХРАНЕНИЯ**.
5) Мод будет загружать декали, палитры, уровни арены, иллюстрации, и тому подобное из этой папки.
6) Если у вас установлена ConfigMachine, в меню настройки модов можно просмотреть, какие регионы активированы, и в каком порядке что загружается.

### <a name="index3"></a>Удаление региона (два варианта)
Вариант 1). Откройте `Rain World\Mods\CustomResources\Your Region\regionInformation.json` и измените параметр activated на false.
Вариант 2). Удалите папку, созданную во время шага 2 (`напр.: Rain World\Mods\CustomResources\The Root`)



### <a name="index4"></a>Как это работает?
Вместо замещения файлов игры файлами региона, вы оставляете регион в отдельной папке. Это поддерживает вашу установку Rain World в чистоте и позволяет использовать несколько кастомных регионов за раз. Можно даже редактировать спауны существ для ванильных или неванильных регионов (достаточно включить в файл world_XX.txt те изменения, которые вам интересны).


### <a name="index5"></a> Конфликты регионов
Мод попытается совместить все регионы, насколько это возможно:
![Mergin visualized](https://cdn.discordapp.com/attachments/473881110695378964/670463211060985866/unknown.png)
Если файл world_XX.txt мода 1 выглядит так:
```
A: C, B, DISCONNECTED
B: A, DISCONNECTED
C: A
```
А world_XX.txt мода 2 так:
```
A: DISCONNECTED, B, C
B: A, DISCONNECTED
D: A
```
Custom Regions совместит их, выдав следующий результат:
```
A: C, B, D
B: A, DISCONNECTED
C: A
D: A
```

### <a name="index6"></a>Полезная информация для моддеров
* CR Сравнивает все переходы между комнатами. Если ваше соединение вступает в конфликт с ванильным (т.е. соответствующий регион первый в списке либо единственный), ванильный переход будет полностью замещён.
```
Analized room [SB_J01 : DISCONNECTED, SB_E02, SB_G03, SB_C07 : SWARMROOM]. Vanilla [True]. NewRoomConnections [SB_ROOTACCESS, SB_E02, SB_G03, SB_C07]. IsBeingReplaced [True]. No Empty Pipes [True]
```
* Если мод вносит изменения в комнату, которая уже изменена другим модом либо добавлена модом, будет произведена попытка слияния 
```
Replaced [SB_J03 : DISCONNECT, SB_J02, SB_F01, SB_S02] with [SB_J03 : SB_ROOTACCESS, SB_J02, SB_F01, SB_S02]
```
* If the room CR is trying to merge doesn't have any DISCONNECTED exits, the two region mods will be incompatible.
```
ERROR! #Found incompatible room [SB_J01 : SB_Q01, SB_E02, SB_G03, SB_C07] from [AR] and [SB_J01 : SB_ROOTACCESS, SB_E02, SB_G03, SB_C07 : SWARMROOM] from [TR]. Missing compatibility patch?
```
* To create the folder structure for you region, just follow the Vanilla structure and create the mod as if you would install it merging files. **Important** If you want to delete a vanilla connection, you must put "DISCONNECTED". (See below for more info)
* Apart from the "`positions.txt`" file for the Region Art, you will need to include a "`depths.txt`" to position the depth of your art. Follows the same order as "`positions.txt`".
* You can include as many layers as you want for the region art.
* You will probably to adjust the positions of the region art again.
* This mod should be compatible with almost anything. If you find any incompabilities contact me.
* HOW TO ADD COMPATIBILITY BETWEEN TWO REGION MODS THAT MODIFY THE SAME ROOM
1) Create a region mod that it is loaded first and modifies a vanilla room by adding new connections:

how the *whole* world_HI.txt from the region NewPipes looks (you only need these lines)
```
ROOMS
HI_A07 : HI_A14, DISCONNECTED, DISCONNECTED, HI_B04, HI_C02
END ROOMS
```
*note* You might have to move around the DISCONNECTED to make sure the vanilla rooms maintains the same layout.

2) Create another region that connects to the vanilla room, but loads after NewPipes

how the *whole* world_HI.txt from the region ModA looks like
```
ROOMS
HI_A07 : HI_A14, HI_B04, HI_C02, HI_MODA, DISCONNECTED
HI_MODA : HI_A07
END ROOMS
```
3) Create another region that connects to the vanilla room, but loads after NewPipes
how the *whole* world_HI.txt from the region ModB looks like
```
ROOMS
HI_A07 : HI_A14, HI_B04, HI_C02, DISCONNECTED, HI_MODB
HI_MODB : HI_A07
END ROOMS
```
![Compatibility patch](https://cdn.discordapp.com/attachments/481900360324218880/758592126786863154/ezgif.com-optimize_1.gif)
<details>
  <summary> How to delete a vanilla connection</summary>

If the vanilla world_XX.txt looks like:
```
	A: C, B, D
```
you want to delete a connection, you must put in your modded world_XX.txt file the following:
```
	A: DISCONNECTED, B, D
```
</details>

### <a name="index7"></a>Известные проблемы
* Система сохранений делает необходимым очистку слота сохранения при установке / отключении новых регионов.

### <a name="index8"></a>Авторы
* Вдохновлено созданным @topicular модом EasyModPack. Началось с простого, зашло в дебри. Пожалуйста, отнеситесь к багам и ошибкам с пониманием.
* Благодарю LeeMoriya за помощь и идеи. Переведено @thalber.