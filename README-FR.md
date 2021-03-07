  
# Custom Regions Support
***
## Vous permet d'installer et de parcourir les packs de régions sans modifier les fichiers du jeu de base et plus encore. Ce mod fonctionne en fusionnant automatiquement les fichiers du monde lors de l'exécution et en redirigeant les accès aux salles.
[![Twitter](https://img.shields.io/twitter/url?style=social&url=https%3A%2F%2Ftwitter.com%2Fgarrakx)](https://twitter.com/garrakx)  [![Downloads](https://img.shields.io/github/downloads/Garrakx/Custom-Regions/total.svg?style=flat-square)]() [![Version](https://img.shields.io/github/release/Garrakx/Custom-Regions.svg?style=flat-square)](https://github.com/Garrakx/Custom-Regions/releases/)
![Custom Regions!](./Images/CRS_thumb.jpg)

## <a name="index"></a>Index

1) [Introduction et FAQ](#FAQ)
2) [Installation du mod CRS](#index1)
3) [Installation d'un pack de région](#index2)
4) [Désinstallation d'un pack de région](#index3)
5) [Écran des packs de CRS](#browserScreen)
6) [Comment ça marche ?](#Index4)
7) [Fusion de régions](#index5)
8) [Pour les moddeurs:](#index6)
    * [STRUCTURE DU DOSSIER](#folder)
    * [COMPATIBILITÉ ENTRE DEUX PACKS](#compatibility)
    * [PUBLIER VOTRE PACK](#publish)
    * [ART DE RÉGION](#art)
    * [PORTES ÉLECTRIQUES](#gates)
    * [PERLES DE DONNÉES PERSONNALISÉES](#pearls)
    * [VIGNETTES](#thumb)
    * [ALBINOS/CRÉATURES COLORÉES](#colors)
    * [DÉBLOQUEUR D'ARÈNES](#arenaUnlock)
9) [Problèmes connus](#issues)
10) [Crédits](#credits)
11) [Journal des modifications](#changelog)
***

### <a name="FAQ"></a>Introduction et FAQ

* **Qu'est-ce que le mod Custom Regions ?**
Custom Region Support (alias `Custom Regions Mod` ou simplement `CRS`) a pour objectif principal d'installer des régions modifiées personnalisées sans modifier les fichiers du jeu. Cela signifie que l'installation est plus simple et que la désinstallation est désormais possible. En outre, CRS fusionne les packs de région afin que vous puissiez en installer plusieurs en même temps.
* **Qu'est-ce qu'un pack de région ?**
Un pack de région (ou simplement un pack) est un mod qui contient une ou plusieurs régions modifiées, ou simplement des modifications des régions de base. Cela signifie qu'un pack peut ajouter deux nouvelles régions au jeu et également modifier les régions de base existantes.
En termes de compatibilité, tout pack de région créé avec la méthode obsolète de fusion avec les fichiers de base fonctionnera avec CRS. D'un autre côté, créer un pack de région apporte spécifiquement des améliorations qui n'étaient pas possibles auparavant.
* **Qu'est-ce qu'une région ?**
Oui, je sais que c'est évident mais passons en revue la définition: une région est un ensemble de pièces, séparé des autres régions par des portes, qui a un acronyme à deux lettres, et son propre fichier `world_XX.txt`. Un pack de région peut inclure plusieurs régions.
* <a name="browser"></a>**Le téléchargeur de packs intégré au jeu**
Nouveau dans les dernières mises à jour, CRS ajoute un téléchageur de packs de région dans le jeu qui vous permet d'installer des packs de régions en un seul clic sans même fermer le jeu. Si un pack est marqué «indisponible», cela signifie que l'auteur ne m'a pas contacté (Garrakx) pour prendre les dispositions nécessaires pour rendre son pack disponible, alors faites-le savoir.
* **Comment exécuter le mod en mode hors ligne**
Si vous ne voulez pas les fonctionnalités en ligne de CRS (téléchargeur de pack, récupérateur d'informations de pack, téléchargeur de vignettes...), vous pouvez placer un fichier texte vide appelé `offline.txt` dans votre dossier de ressources (`Mods\CustomResources\`).
* <a name="packInfo"></a>**Qu'est-ce que `packInfo.json`**
Ce fichier contient des informations sur le pack de région. Après avoir effectué des modifications, vous pouvez redémarrer le jeu pour voir les effets ou utiliser le bouton `RELOAD` situé dans l'écran de configuration / écran de présentation du pack.
  * <u>`regionPackName`</u>: Nom du pack de région. Il est utilisé comme identifiant, il doit donc toujours être le même et ne doit inclure aucun numéro de version. Par exemple:`"Underbelly"` (note: il n'est pas obligé de correspondre à une région du jeu)
  * <u>`description`</u>: Fournissez une description pour votre pack de région, qui apparaîtra dans l'écran de configuration.
  * <u>`author`</u>: Nom(s) du ou des créateur(s) du pack de région.
  * <u>`activated`</u>: Si le pack est activé ou non.
  * <u>`loadOrder`</u>: Il détermine (par rapport aux autres packs) l'ordre dans lequel ce pack de région sera chargé.
  * <u>`regions`</u>: Acronymes à deux lettres des **nouvelles** régions (non présentes dans la version de base) que ce pack inclut, séparées par des virgules. Par exemple: `"GA, MH"`. **Remarque:** votre pack peut ne pas inclure de nouvelles régions s'il est uniquement destiné à modifier les régions de base.
  * <u>`thumbURL`</u>: Lien vers un fichier png qui sera utilisé dans l'écran de présentation de CRS (l'écran de configuration). [Plus d'informations](#browser).
  * <u>`version`</u>: La version du pack. CRS ajoutera "v" avant ce qui apparaît ici, donc `"1.5"` "sera affiché comme `v1.5`.
  * <u>`requirements`</u>: Une description écrite de toute configuration supplémentaire requise par ce pack. `"Nécessite BetterRainbows.dll et ColoredLight.dll."`
  * <u>`checksum`</u>: Chaîne unique de caractères et de nombres générés à partir de tous les fichiers `Properties.txt` et `world_xx.txt`. Cela signifie que si l'un de ces fichiers est modifié ou supprimé, ou si vous ajoutez de nouveaux fichiers, la somme de contrôle changera. Il est utilisé pour voir si le pack de région a reçu des modifications. Ce champ est mis à jour chaque fois que vous rechargez/actualisez les packs de régions.
* <a name="regionInfo"></a>**Différence entre `packInfo.json` et` regionInfo.json`**
Dans les versions récentes de CRS, le fichier `regionInfo.json` a été mis à jour vers `packInfo.json`. Le but est le même, juste un changement de nom pour unifier la dénomination des packs de région. Si vous venez d'une version qui utilisait l'ancien fichier, il devrait être mis à jour automatiquement.
* <a name="corrupted"></a>**Sauvegardes corrompues**
Une fois que vous avez modifié des connexions de salles, que vous avez modifié l'ordre dans lequel les packs sont chargés ou que vous activez / désactivez des packs de région, votre fichier sera corrompu. Dans le meilleur des cas, les tanières de créatures et les objets seront mal placés. Dans le pire des cas, vous ne pourrez pas du tout charger le jeu. Pour résoudre ce problème, vous devez réinitialiser la progression de l'emplacement de sauvegarde à partir du menu d'options.
![Option de réinitialisation de la progression](./Images/reset_progress_button.png)
* **Puis-je utiliser CRS avec une installation de Rain World modifiée ? (régions fusionnées)**
Réponse courte: non.
Réponse longue: peut-être. Actuellement, CRS s'attend à avoir une installation propre, donc les choses peuvent ne pas fonctionner comme prévu lorsque vous avez fusionné des fichiers de région. Il est recommandé de vérifier vos fichiers de jeu avant d'utiliser CRS.
* <a name="hashPearls"></a> **Que sont les données de hachage des perles ?**
CRS et d'autres mods utilisent Enum Extender pour ajouter de nouvelles perles de données. Cependant, l'entier affecté à chaque énumération dépend du système et des régions que vous avez installées. Malheureusement, Rain World enregistre l'ID (identifiant) de données des perles sous forme d'entier. Cela signifie que, par exemple, l'installation d'une nouvelle région peut entraîner le déplacement des perles lors du chargement (au lieu de charger les perles de The Mast, le jeu peut charger les perles de Underbelly). De plus, je ne pouvais pas enregistrer le nom de la perle sous forme de chaîne car cela signifierait que le jeu d'une personne utilisant le pack sans CRS planterait (car il attendait un entier et non une chaîne, et le jeu de base ne gère pas cette erreur). La solution à cela consistait à créer une valeur de hachage du nom de la perle (qui est un entier) et à l'utiliser comme identificateur pour charger la perle. Ce hachage est enregistré dans un fichier car le générer à partir de la même chaîne sur un système différent ne donne pas toujours la même valeur. Tous les packs de région distribués avant cette mise à jour (sans le hachage) devront supprimer toutes les perles de données (à l'aide de devtools) et les replacer, en suivant les instructions mises à jour ci-dessous.
* **Qu'est-ce que RegionDownloader.exe ?**
Pour des raisons techniques, le processus de téléchargement et de décompression est effectué par un programme à part. Si vous ne souhaitez pas utiliser ce programme, exécutez simplement CRS en mode hors ligne (voir ci-dessus).
* **Qu'est-ce que `(packName).crpack` ?**
Il s'agit d'un fichier temporel utilisé par RegionDownloader.exe. Vous pouvez changer l'extension en .zip et l'ouvrir, vous verrez que c'est juste le pack compressé. Vous pouvez supprimer ce fichier en toute sécurité si vous le souhaitez.
* **Pourquoi l'onglet clignote-t-il ?**
S'il s'agit de l'onglet du téléchargeur, cela signifie que vous disposez d'une mise à jour disponible pour un pack. Si c'est l'onglet de l'analyseur, cela signifie qu'il a trouvé une erreur.

***
### <a name="index1"></a>Installer le mod CRS

1) AVANT D'UTILISER TOUT MOD, FAITES UNE COPIE DE VOS SAUVEGARDES (situées dans le dossier `Rain World\UserData`)
2) Téléchargez et installez un chargeur de mod pour Rain World. BepInEx est recommandé. **Tutoriel vidéo** par *LeeMoriya*: [cliquez ici](https://youtu.be/brDN_8uN6-U).
3) Téléchargez la dernière version de CRS depuis [ici](https://github.com/Garrakx/Custom-Regions/releases/).
4) Appliquez **tous** les mods (`CustomRegions.dll`,`EnumExtender.dll`, et enfin `ConfigMachine.dll`) à l'intérieur du fichier [DOWNLOAD_THIS_Custom-Regions-vX.X.zip](https://github.com/Garrakx/Custom-Regions/releases/). Vous recevrez automatiquement des mises à jour. (Remarque: CustomAssets.dll n'est plus nécessaire).
5) Vous avez terminé!

***
### <a name="index2"></a>Installation d'un pack de région

 ***Attention:** La plupart des instructions des régions personnalisées sont obsolètes. Si vous souhaitez utiliser le mod Custom Regions, vous devez suivre ces instructions.*

Si vous souhaitez installer un pack de région à l'aide du téléchargeur du jeu, cliquez simplement sur le bouton de téléchargement et attendez.
1) Exécutez le jeu une fois avec CRS installé et activé. Si tout s'est bien passé, vous devriez voir un nouveau dossier dans Rain World\Mods appelé "`CustomResources`".
2) Créez un nouveau dossier dans Rain World\Mods\CustomResources avec le nom de votre pack de région (par exemple `Rain World\Mods\CustomResources\Underbelly`).
3) Dans ce dossier (c'est-à-dire `Underbelly`), vous devez placer les dossiers "`World`", "`Assets`" et/ou "`Levels`" du pack de région que vous installez.
**Remarque:** Assurez-vous qu'il n'y a pas de dossier intermédiaire dans ce dossier !

	```
	Structure de dossiers correcte:
	├──Mods\CustomResources\
	│          └──Underbelly\
	│                ├── Assets
	│                ├── Levels
	│                ├── World
	¦                ¦
	```

4) Si le pack de région a été créé avec CRS à l'esprit, il devrait être accompagné d'un fichier appelé `packInfo.json` (anciennement appelé `regionInfo.json`, [cliquez ici](#regionInfo) pour plus d'informations). S'il ne l'accompagne pas, Custom Regions Support devrait le créer pour vous. Vous pouvez ouvrir ce fichier avec n'importe quel éditeur de fichier (essayez le bloc-notes (notepad)). Si le fichier était inclus dans le pack de région, les seuls champs dont vous devez vous soucier sont:
    * `activated`: Si ce champ est mis sur "true", cela signifie que le pack de région est activé et sera chargé. Définissez-le sur "false" pour désactiver le pack.
	* `loadOrder`: Il détermine (par rapport aux autres packs) l'ordre dans lequel ce pack de région sera chargé. Vous ne devez le modifier que si vous savez ce que vous faites ou si vous rencontrez des incompatibilités.
**AVERTISSEMENT: toute modification de l'un de ces deux champs (ainsi que l'installation ou la désinstallation de nouvelles régions) corrompra votre sauvegarde et vous devrez réinitialiser la progression à partir du menu d'options du jeu**. [Plus d'informations](#corrupted)

6) Si vous allez à l'écran de configuration alias aperçu des packs de CRS, vous pouvez voir l'ordre dans lequel les packs sont chargés et s'ils sont activés (les régions désactivées apparaîtront avec une vignette sombre, en rouge, et une étiquette "disabled").

***
### <a name="index3"></a>Désinstaller un pack de région (deux options, choisissez-en une)

Option a). Allez dans `Rain World\Mods\CustomResources\Votre Région\packInfo.json` et définissez "activated" sur "false".

Option b). Supprimez le dossier créé à l'étape 2 (c'est-à-dire `Rain World\Mods\CustomResources\Underbelly`)

***
### <a name="browserScreen"></a>Écran des packs de CRS
À l'intérieur de l'écran de Config Machine (accessible via le menu d'options du jeu), vous trouverez la visionneuse du téléchargeur de packs. Ce menu comporte 3 onglets:

   1) Packs actuellement installés: vous indique quels packs de région sont actuellement installés, dans quel ordre et s'ils sont activés / désactivés.
   2) Installation / analyseur de sauvegarde: il est utilisé pour vous donner des informations sur votre sauvegarde et votre installation. Il ne doit être utilisé qu'à titre indicatif, car ses informations peuvent ne pas être exactes.
   3) Téléchargeur de RainWorldModDatabase: un téléchargeur en ligne qui affiche les packs de région disponibles. [Plus d'informations](#browser)

	Accéder à l'écran de configuration

![Accéder au menu de l'écran de configuration](./Images/config_screen_location.png)

***
### <a name="index4"></a>Comment ça marche ?

Avant CRS, la seule façon d'installer les packs de région était de fusionner les fichiers modifiés avec l'installation de base, en modifiant les fichiers du jeu. Cela entraînait une mauvaise compatibilité entre les packs de région et le seul moyen de désinstaller des packs était de réinstaller l'ensemble du jeu.

CRS charge plusieurs packs de région à partir d'un téléchargeur externe et les fusionne au moment de l'exécution comme suit:

1) Tout d'abord, CRS charge les fichiers du monde de base.
2) Ensuite, il charge le premier pack de région (celui avec l'ordre de chargement le plus bas). Si ce pack ajoute de nouvelles connexions, elles seront ajoutées aux connections de base. D'un autre côté, si ce pack a une connexion qui entre en conflit avec le jeu de base, il écrasera la connection de base par celle ajoutée à partir du pack. Cela signifie que vous pouvez modifier n'importe quelle connexion.
	```
	world_xx.txt de base
	[...]
	SALLE1 : SALLE2, SALLE3
	SALLE2 : SALLE4, SALLE1
	SALLE3 : SALLE1
	SALLE4 : SALLE2, DISCONNECTED
	SALLE5 : SALLE6, SALLE7, SALLE8
	[...]
	```
	```
	world_xx.txt du premier pack de région
	[...]
	SALLE1 : SALLE2, DISCONNECTED
	SALLE2 : SALLE4, SALLE1
	SALLE3 : DISCONNECTED
	SALLE4 : SALLE2, ABRI1
	ABRI1 : SALLE4 : SHELTER
	[...]
	```
	Comme nous pouvons le voir, le pack de région modifie les connexions des SALLE1, SALLE3 et SALLE4, et ajoute une nouvelle connexion (ABRI1).
	```
	world_xx.txt fusionné
	[...]
	SALLE1 : SALLE2, DISCONNECTED
	SALLE2 : SALLE4, SALLE1
	SALLE3 : DISCONNECTED
	SALLE4 : SALLE2, ABRI1
	ABRI1 : SALLE4 : SHELTER
	SALLE5 : SALLE6, SALLE7, SALLE8
	[...]
	```
	CRS a écrasé les connexions de SALLE1, SALLE3 et SALLE4, et a ajouté la nouvelle connection de ABRI1.
3) **Important:** Une fois qu'une connexion originale a été écrasée, tous les packs suivants essaieront de fusionner avec elle au lieu de l'écraser. Dans l'exemple ci-dessus, nous pouvons voir que la connexion de SALLE5 n'a pas été modifiée par le premier pack de région, elle est donc toujours considérée comme originale. Cela signifie que si le deuxième pack de région la modifie, il l'écrasera. En revanche, si le deuxième pack tente de modifier les connexions de SALLE3, puisqu'elle a été modifiée par le premier pack, elle n'est plus considérée comme originale et sera donc fusionnée au lieu d'être écrasée.

***
### <a name="index5"></a>Fusion de connexions

Le mod essaiera de fusionner tous les packs de région pour qu'ils soient compatibles:
![Merging visualized](https://cdn.discordapp.com/attachments/473881110695378964/670463211060985866/unknown.png)
Si le fichier world_XX.txt du Mod 1 ressemble à:

```json
A: C, B, DISCONNECTED
B: A, DISCONNECTED
C: A
```

et le fichier world_XX.txt du Mod 2 ressemble à:

```json
A: DISCONNECTED, B, C
B: A, DISCONNECTED
D: A
```

Custom Regions Support fusionnera les deux fichiers et cela ressemblera à ceci:

```json
A: C, B, D
B: A, DISCONNECTED
C: A
D: A
```

***
### <a name="index6"></a>Informations utiles pour les moddeurs

* CRS comparera chaque connexion de pièce. Si votre connexion de pièce est comparée à une connexion originale (c'est-à-dire qu'elle est la première à charger ou la seule installée), CRS remplacera complètement la connexion originale par la connexion modifiée.

```
Analized room [SB_J01 : DISCONNECTED, SB_E02, SB_G03, SB_C07 : SWARMROOM]. Vanilla [True]. NewRoomConnections [SB_ROOTACCESS, SB_E02, SB_G03, SB_C07]. IsBeingReplaced [True]. No Empty Pipes [True]
```

* Si le mod modifie une pièce qui est soit moddée ou modifiée par un autre mod, CRS essaiera de fusionner les deux

```
Replaced [SB_J03: DISCONNECT, SB_J02, SB_F01, SB_S02] by [SB_J03: SB_ROOTACCESS, SB_J02, SB_F01, SB_S02]
```

* Si la salle CR tente de fusionner n'a pas de sortie DÉCONNECTÉE, les deux packs de régions seront incompatibles.

```
ERROR! # Found incompatible room [SB_J01: SB_Q01, SB_E02, SB_G03, SB_C07] from [AR] and [SB_J01: SB_ROOTACCESS, SB_E02, SB_G03, SB_C07: SWARMROOM] from [TR]. Missing compatibility patch?
```

### <a name="folder"></a>STRUCTURE DU DOSSIER

* Pour créer la structure de dossiers pour votre région, suivez simplement la structure des fichiers de base et créez le mod comme si vous l'installiez en fusionnant des fichiers. **Important** Si vous souhaitez supprimer une connexion originale, vous devez mettre "DISCONNECTED".

* Comment supprimer une connexion originale

Si le world_XX.txt original ressemble à:

```
A: C, B, D
```

vous voulez supprimer une connexion, vous devez mettre dans votre fichier moddé world_XX.txt ce qui suit:

```
A: DISCONNECTED, B, D
```

### <a name="compatibility"></a>COMMENT AJOUTER LA COMPATIBILITÉ ENTRE DEUX PACKS DE RÉGION QUI MODIFIENT LA MÊME SALLE

1) Créez un pack de région qui est chargé en premier et modifie une salle originale en ajoutant de nouvelles connexions:
à quoi ressemble le world_HI.txt *entier* de la région NewPipes (vous n'avez besoin que de ces lignes)

```
ROOMS
HI_A07: HI_A14, DISCONNECTED, DISCONNECTED, HI_B04, HI_C02
END ROOMS
```

*note* Vous devrez peut-être vous déplacer dans le DISCONNECTED pour vous assurer que les salles originales conservent la même disposition.

2) Créez une autre région qui se connecte à la salle originale, mais est chargée après NewPipes
à quoi ressemble le world_HI.txt *entier* de la région PackA

```
ROOMS
HI_A07: HI_A14, HI_B04, HI_C02, HI_MODA, DISCONNECTED
HI_MODA: HI_A07
END ROOMS
```

3) Créez une autre région qui se connecte à la salle originale, mais est chargée après NewPipes
à quoi ressemble le world_HI.txt *entier* de la région PackB

```
ROOMS
HI_A07: HI_A14, HI_B04, HI_C02, DISCONNETCED, HI_MODB
HI_MODB: HI_A07
END ROOMS
```

![Compatibility patch](https://cdn.discordapp.com/attachments/481900360324218880/758592126786863154/ezgif.com-optimize_1.gif)

### <a name="publish"></a>COMMENT RENDRE VOTRE PACK DISPONIBLE DANS LE TÉLÉCHARGEUR EN JEU
CRS récupérera et mettra à jour la description locale, la vignette et l'auteur à partir du [lien](http://garrakx.pythonanywhere.com/raindb.json) suivant. Si votre région est absente ou contient des informations erronées, [contactez-moi](https://twitter.com/garrakx). Si vous souhaitez que votre pack soit disponible au téléchargement, vous devez procéder comme suit:

1) Une fois que vous êtes sûr que les fichiers sont définitifs et corrects, fermez le jeu et ouvrez `packInfo.json`.
2) Supprimez tous les nombres et chaînes de la somme de contrôle déposée (laissez simplement `"checksum": ""`). Après avoir réexécuté le jeu, CRS générera une nouvelle somme de contrôle qui représente l'état actuel des fichiers ([cliquez ici](#packInfo) pour plus d'informations sur les champs dans packInfo.json). Notez ce numéro.
3) Faites un zip (assurez-vous que c'est un .zip) avec tous les fichiers nécessaires à l'utilisateur. Le nom du zip **doit être** une correspondance exacte du nom du pack de région. Assurez-vous qu'il n'y ait pas de dossier intermédiaire entre le zip et les dossiers de contenu (`World`,` Assets`, etc.):

	```
	├─Underbelly.zip
	│   ├── Assets
	│   ├── PackDependencies
	│   ├── Levels
	│   ├── World
	¦   ¦
	```
4) Si votre pack nécessite des fichiers .dll supplémentaires, placez-les dans le dossier `PackDependencies`. CRS les déplacera automatiquement vers le dossier des plugins (si l'utilisateur utilise BepInEx) ou vers le dossier Mods.
5) Téléchargez votre fichier sur [mediafire.com](https://www.mediafire.com/) (un compte gratuit est requis). Pour des raisons techniques, Mediafire est le seul site compatible.
6) Contactez-moi avec le lien de téléchargement (c'est-à-dire `https://www.mediafire.com/file/abunchofcharacters/RegionPackName.zip/file`) et la somme de contrôle que vous avez notée.

### <a name="art"></a>ART DE RÉGION

* En plus du fichier "`positions.txt`"pour l'art de région, vous devrez inclure un fichier "`depths.txt`" pour positionner la profondeur de votre art. Celui-ci suit le même ordre que "`positions.txt`".
* Vous pouvez inclure autant de calques que vous le souhaitez pour l'art de région.
* Vous devrez probablement ajuster à nouveau les positions de l'art de région.
* Vous pouvez maintenant déplacer les calques en maintenant `N` et en cliquant avec la souris.
* Pour enregistrer les calques, appuyez simplement sur `B`
* De plus, vous pouvez modifier le fichier texte des positions pendant que le jeu est ouvert et appuyer sur `R` pour voir les changements.

### <a name="gates"></a>PORTES ÉLECTRIQUES

* Pour ajouter une porte électrique, créez un nouveau fichier .txt dans le dossier `Gates` de votre mod (à côté de `locks.txt`) et appelez-le `electricGates.txt`. En suivant le même format que `locks.txt`, écrivez tous les noms de portes qui doivent être électriques suivis de la hauteur du mètre:

```
GATE_SB_AR : 558
```
	Emplacement:`Rain World\Mods\CustomResources\"Votre région"\World\Gates\electricGates.txt`

### <a name="pearls"></a>PERLES DE DONNÉES PERSONNALISÉES

CRS ajoute la possibilité d'ajouter des perles de données personnalisées sans aucun code, et même d'inclure un dialogue. Voici les étapes:

1. Accédez au dossier suivant (`Rain World\Mods\CustomResources\"nom de votre région"\Assets\`). Ici, vous devez créer un fichier texte appelé `pearlData.txt`, ou [télécharger l'exemple](https://github.com/Garrakx/Custom-Regions/blob/master/Example%20Config%20Files/pearlData.txt). Ce fichier indiquera au jeu de créer les perles et de les rendre disponibles dans le menu de placement d'objets de Devtools.

2. Dans `Rain World\Mods\CustomResources\"nom de votre région"\Assets\pearlData.txt`, vous devez indiquer les perles que vous souhaitez créer en suivant cette structure (assurez-vous de la suivre exactement, avec tous les espaces):

```
1: nom_de_la_première_perle: couleurPrincipaleEnHexadécimal: couleurDuRefletEnHexadécimal
2: nom_d_une_autre_perle: couleurPrincipaleEnHexadécimal2: couleurDuRefletEnHexadécimal2
3 ...
```

* Le premier chiffre indique le numéro ID (identifiant) de la perle (plus tard, il déterminera le nom du fichier de dialogue).
* Le deuxième champ est le nom qui apparaîtra dans Devtools, cela peut être n'importe quoi (par exemple: `root_pearl_CC`)
* Le troisième champ est la couleur en hexadécimal (par exemple `00FF00`, utilisez un sélecteur de couleurs en hexadécimal en ligne).
* Le quatrième champ est la couleur du reflet. **Ce n'est pas facultatif**.

3. Lancez le jeu et vérifiez qu'un nouveau champ a été ajouté automatiquement à la fin. Ce sera un gros entier signé. Si vous avez inclu des perles avant que ce numéro n'existe, vous devrez les supprimer avec devtools, enregistrer les modifications et les ajouter à nouveau. Les utilisateurs ont besoin de ce fichier et du fichier `room_Settings.txt` mis à jour pour pouvoir charger les perles. Apprenez la raison technique de ceci [ici](#hashPearls).

*Si vous souhaitez ajouter des perles sans dialogue, vous avez terminé. Si vous voulez un dialogue continuez à suivre les instructions.*

4. Accédez au dossier `Rain World\Mods\CustomResources\"nom de votre région"\Assets\Text\Text_Eng\`. Ici, vous devez créer autant de fichiers texte que de dialogues uniques à créer pour vos perles. En suivant les noms ci-dessus, si je veux ajouter un dialogue pour *nom_de_la_première_perle*, je vais créer un fichier texte appelé `1.txt` (puisqu'il correspond à la première colonne, l'identifiant de la perle). Ouvrez le fichier et écrivez le dialogue. ***N'UTILISEZ PAS LES FICHIERS ORIGINAUX ICI; FAITES UNE SAUVEGARDE***

	Exemple:
```
0-46
Première ligne de la première boîte de dialogue.<LINE>Deuxième ligne de la première boîte de dialogue.

Cette ligne sera affichée dans une deuxième boîte de dialogue!
```
Citant le Rain World Modding Wiki:
`La première ligne du fichier texte doit être **0 - ##**, où **##** correspond au numéro du fichier texte.
Copiez et collez ce fichier dans les autres dossiers de langue (Text_Fre, Text_Ger, etc.). Cela empêchera le jeu de planter si le joueur joue dans une autre langue que l'anglais. (Si vous pouviez réellement traduire le texte pour ces langues, ce serait encore mieux, mais vous n'avez probablement pas de budget de localisation pour votre mod ...)`

5. Exécutez le jeu une fois (avec CRS installé bien sûr). Le jeu cryptera tous les fichiers de dialogue, il est donc plus difficile d'exploiter les données. Vous devez inclure ces fichiers cryptés et tous les autres fichiers créés dans ces étapes lorsque vous rendez votre mod disponible.

### <a name="thumb"></a>VIGNETTES

* Le jeu vérifie d'abord si un fichier appelé `thumb.png` existe (à côté de `regionInfo.json`). Il doit être de taille 360x250.
* Si le jeu ne trouve pas la vignette, il essaiera de la télécharger depuis raindb.net (idem avec les descriptions).
* Si votre mod n'obtient pas automatiquement une vignette ou une description, contactez-moi.

### <a name="colors"></a>ALBINOS / CRÉATURES COLORÉES

* Configurez si une région doit engendrer des créatures albinos (léviathan / jetfish) ou la couleur du Varech Monstre ou du Papa / Frère Longues Jambes.
* Téléchargez [ce fichier](https://github.com/Garrakx/Custom-Regions/blob/master/Example%20Config%20Files/CustomConfig.json) et placez-le à côté du fichier world_XX.txt de la région que vous souhaitez configurer `Rain World\Mods\CustomResources\"Votre région"\World\Regions\"IntialesDeLaRégion"\` (si vous voulez configurer une région orginale, créez simplement un dossier vide avec les initiales de la région et placez-y le fichier).
* Vous devez mettre la couleur au format hexadécimal (exemple: 00FF00).
* Laisser une chaîne vide ("") à côté de la couleur signifie que la couleur originale sera utilisée.
* La chance de la salamandre noire est un nombre compris entre 0 et 1 (par exemple: `0,3`) et détermine la chance qu'une salamande a d'être noire. 1 signifiera un changement de 100% (toutes les salamandres seront noires).

### <a name="arenaUnlock"></a>DÉBLOQUEURS D'ARÈNE

1. Créez un fichier appelé `customUnlocks.txt` et placez-le dans votre `/DossierDuPack/Levels/` à côté de toutes les arènes.
2. Dans ce fichier, vous devez mettre un identifiant pour le débloqueur, suivi de toutes les arènes qui seront déverrouillées.
Exemple:
```
RW1: Mycelium, Tower
RW2: Arène2, Arène3, Arène4
```
3. Placez le ou les objets de déblocage d'arène depuis les outils de développement (Devtools).

Remarque: vous pouvez avoir plusieurs débloqueurs d'arène par région.

***
### <a name="issues"></a>Problèmes connus

* En raison du système de sauvegarde de Rain World, vous devez effacer votre emplacement de sauvegarde si vous désinstallez / installez de nouvelles régions ou modifiez l'ordre de chargement.
* Lorsque vous utilisez le téléchargeur de pack en jeu, il est normal qu'il échoue plusieurs fois. Réessayez jusqu'à ce que cela réussisse.
* La carte du jeu ne fonctionne pas toujours.

***
### <a name="credits"></a>Crédits

Veuillez être patient avec les bugs et les erreurs. L'incroyable vignette / bannière a été faite par [Classick](https://classick-ii.tumblr.com/post/634237640578859008/boopbeep). Merci à LeeMoriya pour son aide et ses suggestions. Merci à Thalber et LB Gamer pour les traductions. Merci Slime_Cubed pour l'idée d'utiliser un processus séparé pour télécharger les packs. Merci à carrotpudding, LB Gamer, Precipitator, Stereo et Laura pour les tests. Merci à dual curly potato noodles pour des suggestions sur la façon de rendre le repo plus conviviale.

***
### <a name="changelog"></a>Journal des modifications

#### [0.8.40] - Janvier 2021

#### Changements

* Mise à jour de `regionInfo.json` en `packInfo.json`.
* Les régions personnalisées / modifiées sont maintenant appelées des packs de région.
* Vous pouvez configurer la chance d'apparition de la salamandre noire. [Plus d'infos](#couleurs)
* Révision de l'écran du pack de région.
* Les packs de région peuvent maintenant être téléchargés et installés en jeu.
* Les graffitis personnalisés peuvent désormais être sélectionnées dans Devtools.
* Ajout du chargement de ressources personnalisées au WWW ctor hook.
* Ajout de déblocables multijoueurs.
* Ajout d'objets multijoueurs supplémentaires à la campagne (Masques de vautours, carcasses de superviseurs, roches fiables...).
* Les packs sans régions ne compteront plus pour le passage du Vadrouilleur.
* Le type des perles de données est stocké sous forme de hachage, récupéré lors du chargement (la couleur de reflet des perles n'est plus facultative) # LES PERLES DE DONNÉES DOIVENT ÊTRE SAUVEGARDÉES À NOUVEAU. [Plus d'infos](#hashPearls).
* Ajout de la possibilité d'avoir plusieurs déblocables par région
* Couleurs des packs modifiées
* Ajout d'un fil d'actualité

#### Corrections

* Correction d'un crash lors de l'utilisation de Partiality (merci Slime_Cubed)
* Correction d'un bug avec les graffitis personnalisés (avoir un fichier qui ne contenait pas ".png" faisait planter le jeu)
* Correction d'un bug dans lequel le titre dans l'écran de région ne se chargeait pas
* Conversation des perles de données optimisée et fixée: Sortie si la région doit être mise à jour, Regarde La Lune ne vous dira plus qu'elle a déjà lu des perles.
* Correction d'un crash lorsqu'un pack de région n'avait pas de dossier `Regions`.
* Correction d'un bug où le fait de ne pas inclure la balise ROOMS empêchait de charger des créatures ou des blocages de chauves-souris.
* Correction d'un bug dans lequel les conditions du niveau de karma des portes de région ne se chargeaient pas.
* Correction d'un crash avec les variations personnalisées.
* Correction de l'onglet de la carte dans Devtools (encore une fois)
* Correction d'un crash avec l'écran de voyage rapide.

#### Technique

* A ajouté de la convivialité pour la collaboration
* CRS rognera l'étiquette du pack sur l'écran de packs lorsqu'elle est trop longue.
* CRS détecte le chargeur de mod que vous utilisez.
* CustomAssets n'est plus nécessaire.
* Algorithme de fusion réécrit: plus stable, plus rapide et plus fiable.
* Le nom du pack apparaîtra dans l'écran de sélection des slugcats.
* Ajout de niveaux de débogage pour le journal de sortie (interne).
* Changement des conventions de nom, suppression de `using` inutiles, ajout de commentaires. 
