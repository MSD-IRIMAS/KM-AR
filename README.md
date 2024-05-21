# KM_AR: Karaoke Mugen for Hololens 2
## Acknolegements
This software uses:
- [Karaoke Mugen](https://gitlab.com/karaokemugen) for fetching karaokes. [(Software licence)](https://gitlab.com/karaokemugen/code/karaokemugen-server/-/blob/master/LICENSE.md) [(Database licence)](https://gitlab.com/karaokemugen/bases/karaokebase/-/blob/master/LICENSE.md)
- [MixedRealityToolKit](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity) for providing building blocks for builing Hololens 2 apps. [(Licence)](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/blob/main/LICENSE.md)
- [NugetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) for managing external packages. [(Licence)](https://github.com/GlitchEnzo/NuGetForUnity/blob/master/LICENSE)
## About local songs
For copyright reasons, no local song are included. If you want to add some, follow the following instructions:
1. Go to https://kara.moe and search for the song you want.
2. With the "Download" button, download the Media and the Subs file and put them in the Assets/Resources folder. (create it if needed)
3. Edit the LocalSongs.json file to match the song's info with the infos needed.
4. If you want to add another song, go back to step 1 but add another entry to LocalSongs.json instead of editing it.
5. When you're done, move the LocalSongs.json file to the Assets/Resources folder.
## Instalation
### Requirements
- A Hololens 2 headset.
- A Visual Studio Environement with the required modules (see [here](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/install-the-tools#installation-checklist)).
### From source
#### Additional requirements
- Unity 2021.3.15f1 with Universal Windows Platform Build Support and Windows Build Support. (IL2CPP)
#### Instructions
1. Download the source.
2. Follow [this](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/unity/new-openxr-project-with-mrtk#set-your-build-target) section of the page.
3. Build.
4. Follow the instructions [here](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-visual-studio) to build the build.
### From build
1. Download the latest release.
2. Follow the instructions [here](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-visual-studio).

</br>
</br>

# KM_AR: Karaoke Mugen pour Hololens 2
## Acquiescements
Ce logiciel utilise:
- [Karaoke Mugen](https://gitlab.com/karaokemugen) pour chercher les karaokes. [(Licence du logiciel)](https://gitlab.com/karaokemugen/code/karaokemugen-server/-/blob/master/LICENSE.md) [(Licence de la base de données)](https://gitlab.com/karaokemugen/bases/karaokebase/-/blob/master/LICENSE.md)
- [MixedRealityToolKit](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity) pour fournie des blocs de construction pour les applications pour Hololens 2. [(Licence)](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/blob/main/LICENSE.md)
- [NugetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) pour gérer les bibliothèques externes. [(Licence)](https://github.com/GlitchEnzo/NuGetForUnity/blob/master/LICENSE)
## À propos des chansons locales
Pour des raisons de droit d'auteur, auccune chanson locale n'est incluse, si vous voulez en ajouter, suivez les instructions suivantes: 
1. Allez sur https://kara.moe et cherchez la chanson que vous voulez.
2. Avec le bouton "Télécharger", téléchargez le fichier média et le fichier sous-titres et mettez les dans le dossier Assets/Resources . (créez-le si besoin)
3. Éditez le fichier LocalSongs.json pour faire correspondre les informations de la chanson avec les informations demandées.
4. Si vous voulez ajouter une autre chanson, retournez à l'étape 1 mais ajoutre une nouvelle entrée à LocalSongs.json au lieu de l'éditer.
5. Quand vous avez fini, déplacez le fichier LocalSongs.json dans le dossier Assets/Resources .
## Instalation
### Prérequis
- Un casque Hololens 2.
- Un environement Visual Studio avec les modules requis (plus d'infos [ici](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/install-the-tools#installation-checklist))
### À partir de la source
#### Prérequis additionels
- Unity 2021.3.15f1 avec le Suport pour la Construction sur la Platforme Univercelle de Windows et le Support de Construction Windows IL2CPP.
#### Instructions
1. Téléchargez la source 
2. Suivez [cette](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/unity/new-openxr-project-with-mrtk#set-your-build-target) section de la page.
3. Construisez
4. Suivez les instructions [ici](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-visual-studio) pour construire ce qui a été construit
### À partir de la constructuion
1. Télécharchez la dernière version.
2. Suivez les instructions [ici](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-visual-studio).