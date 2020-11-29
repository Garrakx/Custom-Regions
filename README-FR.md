
  
# [BETA] Custom-Regions

## Vous permet d'installer des mods ajoutant une ou plusieurs région(s) sans modifier les fichiers du jeu de base et plus encore. Cela fonctionne en fusionnant automatiquement les fichiers du monde lors de l'exécution et en redirigeant les accès aux salles.

![Custom Regions!](https://cdn.discordapp.com/attachments/305139167300550666/777644000529088532/unknown.png)


## Index
* [Installation du mod Custom Regions](#index1)
* [Installation d'une nouvelle région](#index2)
* [Désinstallation](#index3)
* [Comment ça marche ?](#index4)
* [Gestion des conflits - Fusion de régions](#index5)
* [Information pour les moddeurs](#index6)
	* [STRUCTURE DU DOSSIER](#index6.1)
	* [COMMENT AJOUTER LA COMPATIBILITÉ ENTRE DEUX MODS DE RÉGION QUI MODIFIENT LA MÊME PIÈCE](#index6.2)
	* [ART DE RÉGION](#index6.3)
	* [PORTES ÉLECTRIQUES](#index6.4)
	* [PERLES DE DONNÉES PERSONNALISÉES (sans code)](#index6.5)
	* [VIGNETTES](#index6.6)
* [Problèmes connus](#index7)
* [Crédits](#index8)

### <a name="index1"></a>Installation du mod Custom Regions
1) Téléchargez et installez la dernière version de Partiality Launcher depuis [la section "Tools" de RainDB](http://www.raindb.net/).
2) Téléchargez la dernière version CR depuis [ici](https://github.com/Garrakx/Custom-Regions/releases/).
3) Appliquez **tous** (`EnumExtender.dll, ConfigMachine.dll, CustomAssets.dll `(si vous voulez de la musique personnalisée)`et CustomRegions.dll`) les mods dans le fichier `[DOWNLOAD_THIS_Custom-Regions-vX.X. zip]`. Vous recevrez automatiquement les mises à jour.

### <a name="index2"></a>Installer une région
* ***Attention:** La plupart des instructions trouvées dans les régions personnalisées sont obsolètes. Si vous souhaitez utiliser le mod Custom Regions, vous devez suivre ces instructions.*
1) Créez un nouveau dossier dans Rain World\Mods appelé "`CustomResources`"(*si vous exécutez le jeu avec le mod, il sera automatiquement créé*).
2) Créez un nouveau dossier dans Rain World\Mods\CustomResources avec le nom de votre région (`par exemple Rain World\Mods\CustomResources\The Root`). Cela déterminera le nom de la région dans le jeu.
3) Dans ce dossier, vous devez placer les dossiers "`World`","`Assets`" et / ou "`Levels`"de la région que vous installez.
4) Après avoir lancé le jeu, le mod créera un fichier appelé "regionInfo.json". Vous pouvez ouvrir ce fichier avec n'importe quel éditeur de texte (bloc-notes par exemple). Assurez-vous que toutes les informations semblent correctes. Vous pouvez ajouter une description pour chaque région. L'ordre des régions sera utilisé pour déterminer la région à charger en premier. Si vous venez d'une version utilisant regionID.txt, le mod essaiera de le mettre à jour. Pour appliquer des modifications, vous devez redémarrer le jeu ou appuyer sur le bouton de rechargement dans le menu de configuration **VOUS AUREZ BESOIN DE DÉMARRER UNE NOUVELLE SAUVEGARDE APRÈS AVOIR EFFECTUÉ DES CHANGEMENTS**.
5) Le mod chargera les grafittis, les palettes, les arènes, les illustrations, etc. à partir de ce dossier.
6) Vous pouvez vérifier les régions activées (en vert) et l'ordre dans lequel elles sont chargées.

### <a name="index3"></a>Désinstaller une région (deux options, choisissez-en une)
Option a). Allez dans `Rain World\Mods\CustomResources\Votre Région\regionInformation.json` et définissez "activated" sur false.

Option b). Supprimez le dossier créé à l'étape 2 (`par exemple Rain World\Mods\CustomResources\The Root`)



### <a name="index4"></a>Comment ça marche ?
Au lieu de fusionner le nouveau dossier de région dans les fichiers du jeu, vous le placez dans un dossier séparé. Cela gardera votre installation Rain World propre et ajoutera la possibilité de jouer avec plusieurs régions en même temps. Vous pouvez même créer des modifications d'apparation (spawns) pour les régions de base ou les régions personnalisées (il vous suffit d'inclure dans votre fichier world_XX.txt les modifications que vous souhaitez appliquer).


### <a name="index5"></a> Conflits de régions
Le mod essaiera de fusionner tous les mods de région pour qu'ils soient compatibles:
![Mergin visualized](https://cdn.discordapp.com/attachments/473881110695378964/670463211060985866/unknown.png)

Si le fichier world_XX.txt du Mod 1 ressemble à:
```
A: C, B, DISCONNECTED
B: A, DISCONNECTED
C: A
```
et le fichier world_XX.txt du Mod 2 à:
```
A: DISCONNECTED, B, C
B: A, DISCONNECTED
D: A
```
Custom Regions fusionnera les deux fichiers et cela ressemblera à:
```
A: C, B, D
B: A, DISCONNECTED
C: A
D: A
```

### <a name="index6"></a>Informations utiles pour les moddeurs
* CR comparera chaque connexion de pièce. Si la connexion de votre chambre est comparée à une connexion originale (c'est-à-dire qu'elle est la première à charger ou la seule installée), elle remplacera complètement la connexion originale par la connexion modifiée.
```
Analized room [SB_J01 : DISCONNECTED, SB_E02, SB_G03, SB_C07 : SWARMROOM]. Vanilla [True]. NewRoomConnections [SB_ROOTACCESS, SB_E02, SB_G03, SB_C07]. IsBeingReplaced [True]. No Empty Pipes [True]
```
* Si le mod modifie une pièce qui est soit changée ou modifiée par un autre mod, CR essaiera de fusionner les deux.
```
Replaced [SB_J03 : DISCONNECT, SB_J02, SB_F01, SB_S02] with [SB_J03 : SB_ROOTACCESS, SB_J02, SB_F01, SB_S02]
```
* Si la salle que CR essaye de fusionner n'a pas de sortie DISCONNECTED (déconnectée), les deux mods de région seront incompatibles.
```
ERROR! #Found incompatible room [SB_J01 : SB_Q01, SB_E02, SB_G03, SB_C07] from [AR] and [SB_J01 : SB_ROOTACCESS, SB_E02, SB_G03, SB_C07 : SWARMROOM] from [TR]. Missing compatibility patch?
```
### <a name="index6.1"></a>STRUCTURE DU DOSSIER
* Pour créer la structure de dossiers pour votre région, suivez simplement la structure originale et créez le mod comme si vous l'installiez en fusionnant les fichiers. **Important** Si vous souhaitez supprimer une connexion originale, vous devez mettre "DISCONNECTED". 
<details>
  <summary> Comment supprimer une connexion originale</summary>

Si le world_XX.txt original ressemble à:
```
	A: C, B, D
```
vous voulez supprimer une connexion, vous devez mettre dans votre fichier world_XX.txt modifié ce qui suit:
```
	A: DISCONNECTED, B, D
```
</details>

### <a name="index6.2"></a>COMMENT AJOUTER LA COMPATIBILITÉ ENTRE DEUX MODS DE RÉGION QUI MODIFIENT LA MÊME PIÈCE
* Exemple de région qui ajoute une seule pièce à HI (fait par LeeMoriya). [Cliquez ici](https://discordapp.com/channels/291184728944410624/431534164932689921/759459475328860160)
1) Créez un mod de région qui est chargé en premier et modifie une salle originale en ajoutant de nouvelles connexions:

à quoi ressemble le world_HI.txt *entier* de la région NewPipes (vous n'avez besoin que de ces lignes)
```
ROOMS
HI_A07 : HI_A14, DISCONNECTED, DISCONNECTED, HI_B04, HI_C02
END ROOMS
```
*note* Vous pourriez devoir déplacer le DISCONNECTED pour vous assurer que les salles originales conservent la même disposition.

2) Créez une autre région qui se connecte à la salle originale, mais est chargée après NewPipes.

à quoi ressemble le world_HI.txt *entier* de la région ModA
```
ROOMS
HI_A07 : HI_A14, HI_B04, HI_C02, HI_MODA, DISCONNECTED
HI_MODA : HI_A07
END ROOMS
```
3) Créez une autre région qui se connecte à la salle originale, mais est chargée après NewPipes.

à quoi ressemble le world_HI.txt *entier* de la région ModB
```
ROOMS
HI_A07 : HI_A14, HI_B04, HI_C02, DISCONNECTED, HI_MODB
HI_MODB : HI_A07
END ROOMS
```
![Compatibility patch](https://cdn.discordapp.com/attachments/481900360324218880/758592126786863154/ezgif.com-optimize_1.gif)

### <a name="index6.3"></a>ART DE RÉGION
* En plus du fichier "`positions.txt`" pour l'art de la région, vous devrez inclure un "`depths.txt`" pour positionner la profondeur de votre art. Suivez le même ordre que "`positions.txt`".
* Vous pouvez inclure autant de calques que vous le souhaitez pour l'art de la région.
* Vous devrez probablement ajuster à nouveau les positions de l'art de la région.

### <a name="index6.4"></a>PORTES ÉLECTRIQUES
* Pour ajouter une porte électrique, créez un nouveau fichier .txt dans le dossier `Gates` de votre mod (à côté de `locks.txt`) et appelez-le `electricGates.txt`. En suivant le même format que `locks.txt`, écrivez tous les noms des portes qui doivent être électriques suivis de la hauteur de la barre de chargement:
```
GATE_SB_AR : 558
```
(`Rain World\Mods\CustomResources\"Votre Région"\World\Gates\electricGates.txt`)
### <a name="index6.5"></a>PERLES DE DONNÉES PERSONNALISÉES
CR ajoute la possibilité d'ajouter des perles de données personnalisées sans aucun code, et même d'inclure un dialogue. Voici les étapes:
1. Accédez au dossier suivant (`Rain World\Mods\CustomResources\"nom de votre région"\Assets\`). Ici, vous devez créer un fichier texte appelé `pearlData.txt`. Ce fichier indiquera au jeu de créer les perles et de les rendre disponibles dans le menu de placement d'objets de Devtools.
2. Dans `Rain World\Mods\CustomResources\"nom de votre région"\Assets\pearlData.txt`, vous devez indiquer les perles que vous souhaitez créer en suivant cette structure (assurez-vous de la suivre exactement, avec tous les espaces):
```
1 : nom_de_la_première_perle : couleurPrinciapleEnHexadécimal : couleurDeSurbrillanceEnHexadécimal(facultatif)
2 : nom_d_une_autre_perle : couleurPrinciapleEnHexadécimal2
3 ...
```
- Le premier numéro indique le numberID (nombre identifiant) de la perle (il déterminera plus tard le nom du fichier de dialogue).
- Le deuxième champ est le nom qui apparaîtra dans Devtools, cela peut être n'importe quoi (par exemple: `root_pearl_CC`)
- Le troisième champ est la couleur en hexadécimal (par exemple `00FF00`, utilisez un sélecteur de couleurs hexadécimal en ligne).
- Le quatrième champ est facultatif si vous souhaitez que votre perle brille dans une couleur différente.

*Si vous souhaitez ajouter des perles sans dialogue, vous avez terminé. Si vous voulez un dialogue, continuez à suivre les instructions*
3. Accédez au dossier `Rain World\Mods\CustomResources\"nom de votre région"\Assets\Text\Text_Eng\`. Ici, vous devez créer autant de fichiers texte que vous voulez de dialogues uniques pour vos perles. En suivant les noms ci-dessus, si je veux ajouter un dialogue pour *nom_de_la_première_perle*, je vais créer un fichier texte appelé `1.txt` (puisqu'il correspond à la première colonne, l'ID (identifiant) de la perle). Ouvrez le fichier et écrivez le dialogue.  ***N'UTILISEZ PAS LES FICHIERS ORIGINAUX ICI; FAITES UNE SAUVEGARDE***.
Exemple:
```
0-46
Première ligne de la première boîte de dialogue.<LINE>Deuxième ligne de la première boîte de dialogue.

Cette ligne sera affichée dans une deuxième zone de texte !
```
Citant le "Rain World Modding Wiki":
`La première ligne du fichier texte doit être **0-##**, où **##** correspond au numéro du fichier texte.
Copiez et collez ce fichier dans les autres dossiers de langue (Text_Fre, Text_Ger, etc.). Cela empêchera le jeu de planter si le joueur joue dans une autre langue que l'anglais. (Si vous pouviez traduire le texte pour ces langues, ce serait encore mieux, mais vous n'avez probablement pas de budget de localisation pour votre mod...)`

4. Exécutez le jeu une fois (avec CR installé bien sûr). Le jeu cryptera tous les fichiers de dialogue, il est donc plus difficile d'exploiter les données. Vous devrez inclure ces fichiers cryptés et tous les autres fichiers créés dans ces étapes lorsque vous rendrez votre mod disponible.

### <a name="index6.6"></a>VIGNETTES
- Le jeu vérifie d'abord si un fichier appelé `thumb.png` existe (à côté de `regionInfo.json`). Ce fichier doit être de taille 360x250 pixels.
- Si le jeu ne trouve pas la vignette, il essaiera de la télécharger depuis raindb.net (idem pour les descriptions).
- Si votre mod n'obtient pas automatiquement une vignette ou une description, contactez-moi.

### <a name="index7"></a>Problèmes connus
* En raison du système de sauvegarde de Rain World, vous devez effacer votre emplacement de sauvegarde si vous désinstallez / installez de nouvelles régions.
### <a name="index8"></a>Crédits
Vaguement basé sur le mod EasyModPack de @topicular. Ce qui a commencé comme un simple mod est devenu un mod vraiment gros et exigeant. Veuillez être patient avec les bugs et les erreurs. Merci à LeeMoriya pour son aide et ses suggestions.
