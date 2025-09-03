# Jeu d'Échecs WinUI 3

Un jeu d'échecs complet et moderne développé avec WinUI 3, doté d'animations fluides et d'une interface utilisateur élégante.

## 🎯 Fonctionnalités

### ✨ Interface Moderne
- **Design WinUI 3** avec effet Mica backdrop
- **Animations fluides** pour tous les mouvements et interactions
- **Interface responsive** avec panneau de contrôle intégré
- **Thème adaptatif** qui suit les préférences système

### 🏁 Logique de Jeu Complète
- **Mouvements valides** pour toutes les pièces d'échecs
- **Détection d'échec et mat** automatique
- **Gestion du roque** (petit et grand roque)
- **Promotion de pion** automatique en dame
- **Prévention des coups illégaux** (mise en échec de son propre roi)

### 🎮 Contrôles et Interactions
- **Sélection intuitive** des pièces par clic
- **Indicateurs visuels** pour les coups valides
- **Animations de capture** avec rotation et réduction
- **Historique des coups** en temps réel
- **Annulation de coups** (Ctrl+Z)

### 📊 Statistiques et Suivi
- **Compteur de coups** joués
- **Temps de jeu** en temps réel
- **Pièces capturées** comptabilisées
- **Historique complet** des mouvements
- **Indicateur de joueur actuel**

### ⌨️ Raccourcis Clavier
- **F1** : Aide
- **Ctrl+N** : Nouvelle partie
- **Ctrl+Z** : Annuler le dernier coup

## 🏗️ Architecture

### Modèles de Données
- `Position` : Structure pour les coordonnées d'échecs (ligne, colonne)
- `ChessPiece` : Représentation des pièces avec symboles Unicode
- `ChessBoard` : Logique complète du plateau avec validation
- `ChessGame` : Gestion de l'état du jeu et des événements

### Contrôles Visuels
- `ChessSquare` : Cases individuelles avec animations
- `ChessBoard` : Plateau principal avec gestion des mouvements
- `GameControlPanel` : Panneau de contrôle et statistiques

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

### Compilation
```bash
dotnet build
```

### Exécution
```bash
dotnet run
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
├── Models/           # Modèles de données
├── Controls/         # Contrôles personnalisés
├── MainWindow.xaml   # Interface principale
└── App.xaml         # Configuration de l'application
```

### Technologies Utilisées
- **WinUI 3** : Framework d'interface utilisateur moderne
- **C# 12** : Langage de programmation avec nullable reference types
- **XAML** : Déclaration d'interface utilisateur
- **MVVM Pattern** : Architecture modulaire

## 🎯 Fonctionnalités Futures

- [ ] Mode multijoueur en ligne
- [ ] Moteur d'IA pour jouer contre l'ordinateur
- [ ] Sauvegarde et chargement de parties
- [ ] Analyse de position avec évaluation
- [ ] Thèmes personnalisables
- [ ] Sons et effets audio
- [ ] Mode spectateur pour les parties en cours

## 📝 Licence

Ce projet est développé à des fins éducatives et de démonstration.

---

**Développé avec ❤️ en WinUI 3**
