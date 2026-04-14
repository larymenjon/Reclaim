# 📋 CHEAT SHEET - Referência Rápida de Scripts

## 🎯 Quick Lookup (Ctrl+F para procurar)

### 🏢 BUILDING (Edifícios)
```
BuildingData.cs        → Template com propriedades (nome, size, custo)
BuildingSystem.cs      → Lógica de seleção, validação, colocação
Building.cs            → Componente no mundo (instância colocada)

Path: Assets/Scripts/Building/
Padrão: Data → System → Instance
```

### 🛣️ ROAD (Estradas)
```
RoadSystem.cs          → Drag-based placement + connectivity
Road.cs                → Componente no mundo

Path: Assets/Scripts/Road/
Padrão: System → Instance (não tem separate data file)
```

### 🎯 GRID (Grade do mapa)
```
GridManager.cs         → Gerenciador principal (dimensions, cell size, occupancy)
GridCellData.cs        → Dados de uma célula (coordinate, occupancy type)
GridCoordinate.cs      → Struct X,Y
OccupancyType.cs       → Enum (None/Building/Road)

Path: Assets/Scripts/Grid/
Função: Armazena estado do mapa em 2D grid
```

### 👨‍👩‍👧‍👦 FAMILIES (População)
```
FamilyManager.cs       → Spawn, tracking, ciclo de vida
Family.cs              → Dados de família (nome, saúde, fome, morale)
FamilyPresetData.cs    → Template de família
EmploymentStatus.cs    → Enum (Unemployed/Employed/CriticalWorker)

Path: Assets/Scripts/Families/
Padrão: Data → Manager → Class
```

### 🥘 RESOURCES (Comida, Água, etc)
```
ResourceManager.cs     → Tracking de todos recursos + consumo/produção
ResourceType.cs        → Enum (Food/Water/Materials/Energy)
ResourceDefinition.cs  → ScriptableObject com propriedades

Path: Assets/Scripts/Resources/
Função: Gerencia 4 tipos de recursos
```

### ⏱️ TIME (Tempo do jogo)
```
TimeSystem.cs          → Ticks, dias, temperatura, estações

Path: Assets/Scripts/Managers/Survival/
Entrada: Tick cada frame
Saída: TickContext (tick, day, temp)
```

### 💪 MORALE (Esperança da população)
```
MoraleSystem.cs        → Calcula hope baseado em recursos + população + temp

Path: Assets/Scripts/Systems/Survival/
Entrada: NeedsSystem, ResourceManager, Families
Saída: OnHopeChanged event
```

### 💀 NEEDS (Fome, Doença, Morte)
```
NeedsSystem.cs         → Consomem recursos, calculam fome, doença, morte/nasc

Path: Assets/Scripts/Systems/Survival/
Entrada: TickContext, ResourceManager
Saída: Deaths/Births, Health updates
```

### 🔔 EVENTS (Eventos dinâmicos)
```
EventManager.cs        → Trigger eventos baseado em condições
EventData.cs           → ScriptableObject de um evento
EventChoice.cs         → Struct com opções

Path: Assets/Scripts/Events/
Padrão: Data → Manager → Triggers
```

### 🎮 INPUT (Controle do jogador)
```
InputHandler.cs        → Parse keyboard/mouse, raycast hit detection
CameraController.cs    → Câmera RTS (pan, zoom, rotate)

Path: Assets/Scripts/Input/
Entrada: Keyboard/Mouse
Saída: Grid events
```

### 👁️ PREVIEW (Visual feedback)
```
PreviewSystem.cs       → Show/hide ghost preview, valid/invalid colors

Path: Assets/Scripts/Core/
Usado por: BuildingSystem, RoadSystem
```

### 🔙 UNDO (Desfazer)
```
PlacementHistory.cs    → Stack de actions para desfazer

Path: Assets/Scripts/Core/
Usado por: BuildingSystem, RoadSystem
```

### 🧬 STATE (Snapshots)
```
GameStateSnapshot.cs   → Read-only struct com estado atual
TickContext.cs         → Struct passado a cada tick

Path: Assets/Scripts/Core/Survival/
Função: Dados imutáveis para sistemas
```

### 🎨 UI - Menus Principais
```
MenuManager.cs         → Menu principal, navegação de cenas
NewGameSetupManager.cs → Setup (líder/mapa/dificuldade)
NewGameSetupSelectionUI.cs → UI controller do setup

Path: Assets/Scripts/UI/
Entrada: UI buttons
Saída: Scene loads
```

### 🎨 UI - HUD In-Game
```
TopHeaderHudController.cs   → Top bar com stats (pop, recursos, houses)
BuildHudController.cs       → Build/Road/Demolish buttons + Undo

Path: Assets/Scripts/UI/ + Assets/Scripts/Managers/
Função: Display stats + quick action buttons
```

### 🎨 UI - Bottom Menu Sistema
```
BottomCategoryMenuController.cs → Menu de categorias (expandable)
BottomCategoryMenuGroup.cs      → Auto-wire groups (auto-magic)
BottomHudHoverLift.cs           → Animation on hover
BuildingMenuItemButton.cs       → Building option button
RoadMenuItemButton.cs           → Road option button
BuildingTooltipUI.cs            → Dynamic tooltip

Path: Assets/Scripts/UI/Bottom/
Função: Categorized menu system com tooltips
```

### 🎨 UI - Events
```
EventUIBridge.cs       → Bridge EventManager ↔ UI

Path: Assets/Scripts/UI/
Entrada: EventManager.OnEventTriggered
Saída: UI display
```

### 📽️ FLOW (Cenas)
```
IntroSequenceController.cs  → Logos, vídeo intro, splash
LoadingSceneController.cs   → Loading screen + async load

Path: Assets/Scripts/Flow/
Função: Scene transitions
```

### ⚙️ MANAGERS (Orquestradores)
```
Managers/GameManager.cs                    → Build mode orchestrator
Managers/BuildHudController.cs             → UI for build buttons
Managers/Survival/GameManager.cs           → Survival mode orchestrator
Managers/Survival/SurvivalRuntimeBootstrap.cs → Auto-create systems⚠️

Path: Assets/Scripts/Managers/
Função: Wire systems, initialization
```

### 🛠️ EDITOR Utils
```
FixTerrainTrees.cs     → Editor menu para limpar árvores⚠️

Path: Assets/Scripts/Editor/
Status: Candidato a remover
Função: Utility editor (one-time use)
```

---

## 🔥 Scripts por Categoria (Essencialidade)

### ✅ CRÍTICOS (Remover = Game quebra)
```
GridManager, GridCellData, GridCoordinate, OccupancyType
GameMode, PlacementHistory, PreviewSystem
GameStateSnapshot, TickContext
BuildingSystem, Building, BuildingData
RoadSystem, Road
InputHandler, CameraController
FamilyManager, Family, FamilyPresetData
ResourceManager, ResourceType, ResourceDefinition
EventManager, EventData, EventChoice
TimeSystem, NeedsSystem, MoraleSystem
GameManager (Survival), GameManager (Build)
MenuManager, TopHeaderHudController, BuildHudController
EventUIBridge, UI elements
FlowControllers
```

### 🟨 IMPORTANTES (Enhances gameplay)
```
EmploymentStatus.cs
UI/Bottom/* (Category menu system)
NewGameSetupManager + UI
LoadingSceneController
```

### 🟠 OVERHEAD (Pode remover sem quebrar tudo)
```
SurvivalRuntimeBootstrap.cs ⚠️ (Auto-magic bootstrap)
BottomCategoryMenuGroup.cs (Auto-wire - consider explicit)
```

### 🔴 REMOVER (Não essencial)
```
FixTerrainTrees.cs (Editor utility, one-time use)
```

---

## 📊 Contagem Rápida

| Categoria | Scripts | Essencial |
|-----------|---------|-----------|
| Building | 3 | 3 |
| Grid | 4 | 4 |
| Road | 2 | 2 |
| Input | 2 | 2 |
| Core | 5 | 5 |
| Families | 4 | 4 |
| Resources | 3 | 3 |
| Events | 3 | 3 |
| Survival Systems | 3 | 3 |
| Survival Managers | 2 | 1 |
| UI | 11 | 10 |
| Flow | 2 | 1 |
| Editor | 1 | 0 |
| **TOTAL** | **47** | **44** |

---

## 🎯 Cheat Codes para Modificações

### Adicionar novo recurso?
→ `Resources/` pasta

### Criar novo edifício?
→ `Building/BuildingData.cs` (ScriptableObject)

### Criar novo evento?
→ `Events/EventData.cs` (ScriptableObject)

### Modificar população?
→ `Families/FamilyManager.cs` ou `Family.cs`

### Mudar UI?
→ `UI/` pasta apropriada

### Novo sistema de gameplay?
→ `Systems/Survival/` pasta

### Nova cena?
→ `Flow/` controladores

### Câmera/Input?
→ `Input/` pasta

### Grid logic?
→ `Grid/GridManager.cs`

---

## 🔗 Patternos Padrão

### Manager + System Pattern
```
Manager     = Orquestra (GameManager, FamilyManager)
System      = Executa (TimeSystem, NeedsSystem)
```

### ScriptableObject Pattern
```
*Data.cs    = Template (BuildingData, EventData, FamilyPresetData)
*Manager.cs = Lógica (BuildingSystem, EventManager)
*.cs        = Instance (Building, Road, Family)
```

### Event-Driven UI
```
System.OnSomethingHappened
    ↓
UIComponent listens
    ↓
UIComponent updates
```

### Composition Root Pattern
```
GameManager
    ↓ 
    wires e inicia
    ↓
Todos os systems
```

---

## 📍 Encontrar Arquivo Rápido

Procurando por "x"?

| Se quer... | Vá para... |
|-----------|-----------|
| Edifícios | `Building/` |
| Estradas | `Road/` |
| Grade | `Grid/` |
| População | `Families/` |
| Recursos | `Resources/` |
| Tempo | `Managers/Survival/TimeSystem.cs` |
| Necessidades | `Systems/Survival/NeedsSystem.cs` |
| Morale | `Systems/Survival/MoraleSystem.cs` |
| Eventos | `Events/` |
| Input | `Input/` |
| UI | `UI/` |
| Cenas | `Flow/` |
| Prévis | `Core/PreviewSystem.cs` |
| Undo | `Core/PlacementHistory.cs` |

---

## 💡 Dicas Rápidas

- **Use Ctrl+H** para Find & Replace
- **Use Ctrl+,** para VS Code Settings
- **Use Ctrl+Shift+F** para Find across files
- **Use Ctrl+.** para Quick Fix/Refactor
- **Use F12** para Go to Definition
- **Adicione comentários XML** a cada script
- **Sempre test após modify** Manager ou System
- **Dispatch eventos** em vez de chamar direto
- **Escuta TickContext** se roda cada frame
- **Use ScriptableObjects** para dados reutilizáveis

---

## 🎓 Ordem Recomendada Leitura

1. TimeSystem.cs
2. GameManager (Survival)
3. NeedsSystem.cs
4. MoraleSystem.cs
5. EventManager.cs
6. FamilyManager.cs
7. ResourceManager.cs
8. GridManager.cs
9. BuildingSystem.cs
10. RoadSystem.cs
11. InputHandler.cs
12. UI components

---

## ✅ Checklist para Novo Dev

- [ ] Leu este arquivo
- [ ] Leu SCRIPT_ANALYSIS.md
- [ ] Estudou ARCHITECTURE_VISUAL_MAP.md
- [ ] Salvou QUICK_REFERENCE_GUIDE.md
- [ ] Entende padrão Manager + System
- [ ] Entende event-driven UI
- [ ] Já abriu GameManager(Survival)
- [ ] Já abriu TimeSystem.cs
- [ ] Pronto para modifier código! 🚀

---

**TL;DR:** 
- **47 scripts**, **44 essenciais**
- Remova: `FixTerrainTrees.cs`, `SurvivalBootstrap.cs`
- Renomeie: `GameManager.cs` → `BuildGameManager.cs`
- Adicione: XML docstrings
- Padrão chave: Manager + System, ScriptableObject Data
- Leia em ordem: TimeSystem → GameManager → NeedsSystem

🎮 **Projeto excelente, só precisa limpeza final!**
