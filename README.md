# Jeu d'Échecs WinUI 3

Un jeu d'échecs complet et moderne développé avec WinUI 3, doté d'animations fluides, d'une interface utilisateur élégante et d'un système de persistance de données avec Entity Framework.

## 🎯 Fonctionnalités

### ✨ Interface Moderne
- **Design WinUI 3** avec effet Mica backdrop
- **Animations fluides** pour tous les mouvements et interactions
- **Interface responsive** avec navigation intuitive
- **Thème adaptatif** qui suit les préférences système
- **Layout épuré** avec cartes modernes et coins arrondis

### 🏁 Logique de Jeu Complète
- **Mouvements valides** pour toutes les pièces d'échecs
- **Détection d'échec et mat** automatique
- **Gestion du roque** (petit et grand roque)
- **Promotion de pion** automatique en dame
- **Prévention des coups illégaux** (mise en échec de son propre roi)
- **Demande de match nul** avec confirmation
- **Gestion des états** : Échec, Mat, Pat, Match nul

### 🎮 Contrôles et Interactions
- **Sélection intuitive** des pièces par clic
- **Indicateurs visuels** pour les coups valides
- **Animations de capture** avec rotation et réduction
- **Historique des coups** en temps réel
- **Boutons de contrôle** : Sauvegarder, Demander match nul, Retour au menu
 
### 💾 Persistance de Données
- **Base de données SQL Server** avec Entity Framework Core
- **Sauvegarde automatique** des positions des pièces
- **Chargement de parties** avec restauration exacte de l'état
- **Gestion des parties sauvegardées** avec interface dédiée
- **Fallback en mémoire** si la base de données n'est pas disponible

### 📊 Statistiques et Suivi
- **Compteur de coups** joués
- **Historique complet** des mouvements
- **Indicateur de joueur actuel** avec noms personnalisés
- **Statut du jeu** en temps réel (En cours, Échec, Mat, etc.)
- **Informations des parties** : Joueurs, date de création, dernière partie

### ⌨️ Raccourcis Clavier
- **F1** : Aide
- **Ctrl+N** : Nouvelle partie

## 🏗️ Architecture

### Modèles de Données
- `Position` : Structure pour les coordonnées d'échecs (ligne, colonne)
- `ChessPiece` : Représentation des pièces avec symboles Unicode
- `ChessBoard` : Logique complète du plateau avec validation
- `ChessGame` : Gestion de l'état du jeu et des événements
- `GameInfo` : Informations sur une partie (joueurs, date, etc.)
- `SavedGameInfo` : Informations sur une partie sauvegardée

### Contrôles Visuels
- `ChessSquare` : Cases individuelles avec animations
- `ChessBoard` : Plateau principal avec gestion des mouvements
- `GamePage` : Page de jeu avec contrôles intégrés
- `HomePage` : Page d'accueil avec gestion des parties

### Persistance de Données
- `ChessDbContext` : Contexte Entity Framework pour SQLite
- `SavedGame` : Modèle pour les parties sauvegardées
- `SavedChessMove` : Modèle pour les mouvements sauvegardés
- `BoardState` : Modèle pour l'état du plateau
- `IGameDataService` : Interface pour les services de données
- `EntityFrameworkGameDataService` : Implémentation avec EF Core
- `SimpleGameDataService` : Implémentation en mémoire (fallback)

### Animations
- **Sélection de pièce** : Animation d'échelle et d'opacité
- **Mouvement de pièce** : Translation fluide avec effet d'échelle
- **Capture** : Rotation et réduction de la pièce capturée
- **Indicateurs** : Apparition animée des coups valides
- **Échec** : Animation de couleur pour le roi en échec

## 🚀 Installation et Exécution

### Prérequis
- Windows 10 version 1903 ou ultérieure
- .NET 8.0 SDK
- Visual Studio 2022 ou Visual Studio Code
- Windows App SDK 1.7 ou ultérieur

### Compilation
```bash
dotnet build
```

### Exécution
```bash
dotnet run
```

### Base de Données
L'application utilise SQL Server LocalDB avec Entity Framework Core. La base de données est automatiquement créée au premier lancement.

**Prérequis pour la base de données :**
- SQL Server LocalDB (inclus avec Visual Studio)
- Ou SQL Server Express/Standard/Enterprise

**Chaîne de connexion :**
```
Server=(localdb)\mssqllocaldb;Database=ChessGameDB;Trusted_Connection=true;MultipleActiveResultSets=true
```

## 🎨 Design et UX

### Couleurs et Thème
- **Cases claires** : #F0D9B5
- **Cases sombres** : #B58863
- **Accent** : Couleur d'accent système
- **Adaptatif** : Suit automatiquement le thème sombre/clair

### Animations
- **Durée** : 200-400ms selon le type d'animation
- **Easing** : CubicEase, BackEase, ElasticEase pour des transitions naturelles
- **Performance** : Optimisées pour une fluidité maximale

### Accessibilité
- **Contraste élevé** pour une meilleure lisibilité
- **Indicateurs visuels** clairs pour l'état du jeu
- **Raccourcis clavier** pour une navigation rapide

## 🔧 Développement

### Structure du Projet
```
Jeu D'echec/
├── Models/           # Modèles de données du jeu
├── Controls/         # Contrôles personnalisés (HomePage, GamePage, ChessBoard)
├── Data/             # Modèles Entity Framework et contexte
├── Services/         # Services de persistance de données
├── MainWindow.xaml   # Interface principale
└── App.xaml         # Configuration de l'application
```

### Technologies Utilisées
- **WinUI 3** : Framework d'interface utilisateur moderne
- **C# 12** : Langage de programmation avec nullable reference types
- **XAML** : Déclaration d'interface utilisateur
- **Entity Framework Core** : ORM pour la persistance de données
- **SQL Server** : Base de données relationnelle (LocalDB pour le développement)
- **MVVM Pattern** : Architecture modulaire

## 🎯 Fonctionnalités Futures

- [ ] Mode multijoueur en ligne
- [ ] Moteur d'IA pour jouer contre l'ordinateur
- [x] ~~Sauvegarde et chargement de parties~~ ✅ **Implémenté**
- [ ] Analyse de position avec évaluation
- [ ] Thèmes personnalisables
- [ ] Sons et effets audio
- [ ] Mode spectateur pour les parties en cours
- [ ] Export/Import de parties (PGN)
- [ ] Statistiques détaillées des joueurs
- [ ] Mode tournoi

## 🆕 Fonctionnalités Récentes

### Version Actuelle
- ✅ **Système de persistance complet** avec Entity Framework Core
- ✅ **Sauvegarde des positions exactes** des pièces sur le plateau
- ✅ **Interface de gestion des parties** sauvegardées
- ✅ **Demande de match nul** avec confirmation entre joueurs
- ✅ **Popup de confirmation** lors de la sauvegarde
- ✅ **Fallback en mémoire** si la base de données n'est pas disponible
- ✅ **Layout épuré** sans boutons inutiles
- ✅ **Navigation robuste** avec gestion d'erreurs

### Améliorations Techniques
- **Architecture modulaire** avec services de données
- **Gestion d'erreurs** robuste avec fallbacks
- **Interface utilisateur** modernisée et simplifiée
- **Base de données SQLite** pour la persistance locale
- **Modèles de données** complets pour les parties et mouvements

## 📝 Licence

Ce projet est développé à des fins éducatives et de démonstration.

---

**Développé avec ❤️ en WinUI 3**
