# 🏗️ ARQUITETURA DO PROJETO - Mapa Visual

## 📐 Diagrama de Fluxo - Como o Jogo Inicia

```
┌─────────────────────────────────────────────────────────┐
│               GAME STARTUP SEQUENCE                      │
└─────────────────────────────────────────────────────────┘

1️⃣  IntroSequenceController
    📹 Studio logos → Vídeo intro → Splash screen
         ↓
    (Pressione qualquer botão ou espere)

2️⃣  MenuManager @ MainMenu Scene
    🎮 Novo Jogo / Carregar / Opções
         ↓ [Click: Novo Jogo]

3️⃣  NewGameSetupManager @ NewGameSetup Scene
    ⚙️ Seleciona: Líder + Mapa + Dificuldade
    ↓
    NewGameSetupSelectionUI (UI visual)
         ↓ [Click: Começar]

4️⃣  LoadingSceneController @ LoadingScene
    ⏳ Async load GameScene
    💡 Mostra tips & slides
         ↓ [Async complete]

5️⃣  GameScene + GameManager (Survival)
    ✨ Jogo começa!
```

---

## 🎮 Diagrama de Sistemas - Em Tempo de Jogo

```
┌─────────────────────────────────────────────────────────────────┐
│                      GAME MANAGER (Survival)                     │
│              Master orchestrator do gameplay                      │
└─────────────────────────────────────────────────────────────────┘

                              │
                ┌─────────────┼─────────────┐
                │             │             │
         ┌──────▼─────┐  ┌───▼────────┐  ┌─▼──────────┐
         │  TIME      │  │ POPULATION │  │ RESOURCES  │
         │  SYSTEM    │  │  SYSTEM    │  │  SYSTEM    │
         └──────┬─────┘  └───┬────────┘  └─┬──────────┘
                │             │             │
         Ticks/Days      Families    Food/Water/Materials/Energy
         Temperature     Employment  Starting amounts
         Season cycle    Illness     Consumption rates
                │             │             │
                └─────────────┼─────────────┘
                              │
                        ┌─────▼─────┐
                        │   NEEDS    │
                        │  SYSTEM    │
                        └─────┬─────┘
                              │
                    Fome/Sede/Doença
                    Morte/Nascimento
                    Starvation penalties
                              │
                        ┌─────▼──────┐
                        │  MORALE     │
                        │  SYSTEM     │
                        └─────┬───────┘
                              │
                        Hope/Morale
                        (baseado em resources
                         + população +
                         + evento eventos)
                              │
                        ┌─────▼──────┐
                        │   EVENT     │
                        │  MANAGER    │
                        └─────┬───────┘
                              │
                    Triggers eventos dinâmicos
                    (Random ou baseado em condições)
                              │
                        ┌─────▼──────┐
                        │  EVENT UI   │
                        │  BRIDGE     │
                        └─────┬───────┘
                              │
                        Mostra ao jogador
```

---

## 🏗️ Diagrama de Sistemas Paralelos - GRID + BUILDING + ROAD

```
┌─────────────────────────────────────────────────────────────────┐
│                   INPUT HANDLER                                   │
│         Converte mouse/teclado em grid events                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                ┌─────────────┼─────────────┐
                │             │             │
         ┌──────▼──────┐  ┌───▼────────┐  ┌──▼───────────┐
         │   CAMERA    │  │   GRID     │  │  PLACEMENT   │
         │ CONTROLLER  │  │  MANAGER   │  │   HISTORY    │
         └──────┬──────┘  └───┬────────┘  │  (Undo)      │
                │             │          └──┬───────────┘
         RTS camera      Grid dimensions     │
         Pan/Zoom        Células (X,Y)       └─ Desfazer
         Rotação         Ocupação (None/    placement
                         Building/Road)

                │             │
                └─────────────┼─────────────┐
                              │             │
                        ┌─────▼────────┐   │
                        │  BUILDING    │   │
                        │  SYSTEM      │   │
                        └─────┬────────┘   │
                              │            │
                 Selection → Validation    │
                 Preview (via PreviewSys) │
                 Placement cost        │
                              │            │
                     ┌────────▼──────┐    │
                     │ Building.cs    │    │
                     │ (Instância)    │    │
                     │ Stores data:   │    │
                     │ - Building ID  │    │
                     │ - Origin cell  │    │
                     │ - Rotation     │    │
                     │ - Occupied     │    │
                     │   cells        │    │
                     └────────┬───────┘    │
                              │            │
                ┌─────────────┴────────┐   │
                │                      │   │
            Grid updated          ┌───▼──▼────┐
            Occupancy marked      │  PREVIEW  │
                                  │  SYSTEM   │
                                  │ (Ghost)   │
                                  │ Valid/    │
                                  │ Invalid   │
                                  │ Colors    │
                                  └───┬──────┘
                                      │
                                   Visual
                                   feedback
                │
         ┌──────▼──────┐
         │   ROAD      │
         │  SYSTEM     │
         └──────┬──────┘
                │
         Drag-based placement
         Neighbor connectivity
         Cost per cell
                │
         ┌──────▼──────┐
         │   Road.cs   │
         │ (Instância) │
         │ Stores:     │
         │ - Connection│
         │   mask      │
         │   (N/E/S/W) │
         │ - Cost      │
         └─────────────┘
```

---

## 🎨 Diagrama de UI - Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                    SCREEN / CANVAS                               │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ TopHeaderHudController (Top Bar)                                 │
│ ┌──────────────────────────────────────────────────────────┐   │
│ │ Population │ 🥖 Food │ 💧 Water │ 📦 Materials │ 🔋 Energy │   │
│ │  🏠 Houses │ 🌍 Terrain │ 📊 Stats │ 🔫 Ammo │ ⚔️ Defense  │   │
│ └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘

               ┌─────────────────────────┐
               │    GAME WORLD (3D)      │
               │   Grid + Buildings      │
               │   + Roads + Terrain     │
               └─────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ BuildHudController (Left/Bottom Area)                            │
│ [🏢 Build] [🛣️ Build Road] [🔨 Demolish] [◀ Undo]              │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ BottomCategoryMenuController (Bottom Right - Expandable)         │
│ ┌───────────────┬─────────────┬──────────────┐                  │
│ │  Buildings    │   Roads     │   Options    │                  │
│ ├───────────────┼─────────────┼──────────────┤                  │
│ │  Category     │  Category   │  (etc)       │                  │
│ │   (hovers)    │   (hovers)  │              │                  │
│ └───────┬───────┴─────┬───────┴──────────────┘                  │
│         │             │                                          │
│    ┌─────────┐   ┌──────────┐                                    │
│    │ Building│   │Building  │                                    │
│    │MenuItem │   │Tooltip   │                                    │
│    │Button   │───│UI        │                                    │
│    │         │   │(Dynamic) │                                    │
│    └─────────┘   └──────────┘                                    │
│                                                                  │
│    ┌──────────┐                                                  │
│    │Road Menu │                                                  │
│    │Button    │                                                  │
│    └──────────┘                                                  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ EventUIBridge - Mostra eventos dinâmicos quando ocorrem          │
│ ┌──────────────────────────────────────────────────┐            │
│ │  ⚠️ EVENTO: Resource Crisis                      │            │
│ │  Sua população está sofrendo com falta de água!  │            │
│ │  [ Opção 1 ]  [ Opção 2 ]  [ Opção 3 ]         │            │
│ └──────────────────────────────────────────────────┘            │
│ (Mostra quando EventManager triggers um evento)                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Diagrama de Ciclo de Jogo - Cada Tick

```
┌──────────────────────────────────────────────────────────────┐
│                    GAME TICK (1 frame)                        │
└──────────────────────────────────────────────────────────────┘

1. TimeSystem.Tick()
   ├─ Incrementa tick counter
   ├─ Atualiza temperature (sim. sazonal)
   └─ Cria TickContext (contém tick, day, temp, dt)

                    ↓

2. NeedsSystem.ProcessTick(TickContext)
   ├─ Cada família consome Food/Water
   ├─ Calcula fome/sede
   ├─ Atualiza saúde (doença progride)
   ├─ Verifica starvation → morte
   ├─ Verifica nascimento se condiçõesboas
   └─ Atualiza counters de illness

                    ↓

3. MoraleSystem.RecalculateHope()
   ├─ Lê estado população (NeedsSystem)
   ├─ Lê recursos (ResourceManager)
   ├─ Lê temperaturatemperaturas (TimeSystem)
   ├─ Calcula formula:
   │  Hope = baseHope
   │        + bonusFromResources
   │        + penaltyFromStarvation
   │        + penaltyFromSickness
   │        + bonusFromPopulationHealth
   └─ Dispatch OnHopeChanged event

                    ↓

4. EventManager.CheckTriggers()
   ├─ Para cada EventData:
   │  ├─ Verificar se condições met
   │  ├─ Verificar cooldown não active
   │  └─ Se todas conditions OK:
   │     ├─ Trigger evento
   │     ├─ Dispatch OnEventTriggered
   │     └─ Setar cooldown
   └─ EventUIBridge catches e mostra UI

                    ↓

5. FamilyManager.ProcessPopulationChanges()
   └─ Atualiza counts baseado em:
      - Deaths (NeedsSystem)
      - Births (NeedsSystem)
      - Job assignments
      - Diseases

                    ↓

6. ResourceManager.ApplyConsumption()
   ├─ Subtrai consumo base
   ├─ Aplica produção (se existir)
   └─ Registra overflow/shortage

                    ↓

7. OnTickFinished event
   └─ UI atualiza com novos valores

                    ↓

[Frame render]
[Aguarda próximo frame = próximo tick]
```

---

## 🧩 Diagrama de Dependências - Quem Depende de Quem

```
Legend:
→ Depende de
⟷ Bidirecional

┌─────────────────────────────────────────────────────────┐
│ GAME MANAGER (Survival)                                 │
│ - Orquestra tudo                                        │
│ - Wira e inicializa sistemas                            │
└─────────────┬───────────────────────────────────────────┘
              │
     ┌────────┼────────┬───────────┬────────────┐
     ↓        ↓        ↓           ↓            ↓
     │    TimeSystem   │      FamilyManager  EventManager
     │       │         │           │            │
     │    TickContext  │    Family (class)      │
     │                 │    EmploymentStatus    │
                       │                        EventData
                       │                        EventChoice
                       └─────────┬──────────────┘
                                 │
     ┌───────────────────────────┼────────────────────────┐
     ↓                           ↓                        ↓
 NeedsSystem            MoraleSystem          EventUIBridge
     │                       │                        │
 Consome recursos        Lê estado            Mostra UI
 Calcula fome/doença     Calcula hope         
 Morte/Nascimento                            
     │                       │
     ├───────────────────────┤
             ↓ ↓
        ResourceManager
          │
     Food/Water/Materials
     Energy tracking
     Production/Consumption


GRID SYSTEM (Paralelo):
┌──────────────────────┐
│  GameManager (Build) │
│  (Managers/)         │
└──────┬───────────────┘
       │
┌──────▼─────────────────────────────┐
│ GridManager                         │
│ - Dimensions                        │
│ - Cell Size                         │
│ - Occupancy array                   │
└──┬──────────────────┬───────────────┘
   │                  │
   ↓                  ↓
InputHandler      BuildingSystem ⟷ RoadSystem
   │                  │                  │
   ├─→ CameraController  ├─→ PreviewSystem
   │                  │                  │
Raycast detect    Building.cs        Road.cs
Parse input       BuildingData       RoadData
                      │
                PlacementHistory
                (Undo support)
```

---

## 📊 Dependency Graph (Matrix)

Quem precisa de quem para funcionar:

```
                      Grid  Build Road  Time  Need Moral Event Fam  Res  UI
GameManager (S)        ✓     -    -     ✓     ✓    ✓    ✓     ✓    ✓    -
GridManager            -     -    -     -     -    -    -     -    -    -
BuildingSystem         ✓     -    -     -     -    -    -     -    -    -
RoadSystem             ✓     -    -     -     -    -    -     -    -    -
TimeSystem             -     -    -     -     -    -    -     -    -    -
NeedsSystem            -     -    -     ✓     -    -    -     ✓    ✓    -
MoraleSystem           -     -    -     ✓     ✓    -    -     ✓    ✓    -
EventManager           -     -    -     ✓     ✓    ✓    -     -    -    -
EventUIBridge          -     -    -     -     -    -    ✓     -    -    ✓
FamilyManager          -     -    -     -     -    -    -     -    -    -
ResourceManager        -     -    -     -     -    -    -     -    -    -
```

---

## 🎯 Data Flow - Como Dados Fluem

```
USER INPUT (Mouse/Keyboard)
         │
         ↓
    InputHandler
    (Parse + Raycast)
         │
    ┌────┼────┬────────────┐
    │    │    │            │
    ↓    ↓    ↓            ↓
 Build  Road Demo      Camera
 Click  Click Click      Move
    │    │    │            │
    └────┼────┴─────────────┘
         │
    GameManager.SetMode()
         │
    ┌────┴────┬─────────┐
    │         │         │
    ↓         ↓         ↓
Building   Road      Demolish
System     System     Mode
    │         │
    └────┬────┘
         │
      Preview
      System
    (Ghost visual)
         │
         ↓
      Grid.Update()
      (Marca célula como ocupada)
         │
         ↓
    Building/Road.cs
    (Componente MonoBehaviour)
    Armazenado na cena


GAME SIMULATION (Tick Loop):
         │
TimeSystem.Tick()
         │
    TickContext
    (contém tick, day, temp)
         │
    ┌────┴──────┬──────────┬────────┐
    ↓           ↓          ↓        ↓
NeedsSystem MoraleSystem  Family-  Event-
Calcula     Calcula Hope  Manager  Manager
Fome                      Updates  Checks
Doença                    Popul.   Triggers
Morte/Nasc.               Emprego
         │           │      │
         ├───────────┼──────┤
         ↓           ↓      ↓
    GameStateSnapshot
    (Read-only game state)
         │
         ↓
    UI Update
    (TopHeaderHud)
    (EventUIBridge)
    mostra novos valores
```

---

## 📋 Tabela de Responsabilidades por Script

| Sistema | Arquivo | Responsabilidade | Input | Output |
|---------|---------|------------------|-------|--------|
| **Time** | TimeSystem.cs | Ticks, dias, temp, estações | (nenhum) | TickContext, OnDayChanged |
| **Population** | FamilyManager.cs | Spawn, track families | TimeSystem | FamilyList, homeless count |
| **Needs** | NeedsSystem.cs | Fome, doença, morte | TickContext, ResourceManager | Deaths/Births, Health updates |
| **Morale** | MoraleSystem.cs | Calc hope | NeedsSystem, Fam, Res | OnHopeChanged |
| **Events** | EventManager.cs | Trigger eventos | Morale, Res, Fam, Time | OnEventTriggered, choices |
| **Resources** | ResourceManager.cs | Track Food/Water/Mat/Energy | NeedsSystem | Amounts, shortages |
| **Grid** | GridManager.cs | Grid cells ocupação | BuildingSystem, RoadSystem | Grid data, occupancy |
| **Building** | BuildingSystem.cs | Build mode logic | InputHandler | Building placed |
| **Road** | RoadSystem.cs | Road mode logic | InputHandler | Road placed |
| **Input** | InputHandler.cs | Parse user input | Mouse/Keyboard | Grid events |
| **Camera** | CameraController.cs | Camera control | Mouse/Keyboard | Camera transform |
| **Preview** | PreviewSystem.cs | Ghost visual | BuildingSystem, RoadSystem | Visual ghost |
| **History** | PlacementHistory.cs | Undo | BuildingSystem, RoadSystem | Undo capability |
| **UI Flow** | MenuManager.cs | Scene navigation | UI buttons | Scene loads |
| **UI HUD** | TopHeaderHud.cs | Display stats | GameManager.Survival | Visual numbers |
| **UI Events** | EventUIBridge.cs | Show event UI | EventManager | Event choices |

---

## ✨ Key Insights

1. **TimeSystem é o iniciador**
   - Tudo começa com ticks
   - Outros sistemas escutam TickContext

2. **NeedsSystem é o coração**
   - Causa morte/nascimento
   - Consome recursos
   - Afeta morale

3. **Grid é a base física**
   - Buildings e Roads existem em células
   - Limita o que pode ser colocado

4. **EventManager é dinâmico**
   - Reage a condições do jogo
   - Cria imprevisibilidade

5. **UI é passiva**
   - Escuta eventos
   - Reflete estado, não controla

---

**Este mapa visual deve ajudar a entender como tudo se conecta!** 🗺️
