# 🔍 QUICK REFERENCE GUIDE - Navegando o Código

## 📚 Onde Encontrar O Quê

### "Eu quero modificar..."

#### ✏️ Como os edifícios funcionam
Vá para:
- `Assets/Scripts/Building/BuildingData.cs` - Propriedades (tamanho, custo, prefab)
- `Assets/Scripts/Building/Building.cs` - Componente da instância
- `Assets/Scripts/Building/BuildingSystem.cs` - Lógica de colocação

Fluxo: BuildingData (template) → BuildingSystem (lógica) → Building (instância)

---

#### 🛣️ Como estradas funcionam
Vá para:
- `Assets/Scripts/Road/Road.cs` - Instância de estrada
- `Assets/Scripts/Road/RoadSystem.cs` - Lógica drag-based

Fluxo: InputHandler → RoadSystem → Road instâncias

---

#### 👨‍👩‍👧‍👦 Famílias e população
Vá para:
- `Assets/Scripts/Families/Family.cs` - Dados de família
- `Assets/Scripts/Families/FamilyManager.cs` - Ciclo de vida
- `Assets/Scripts/Families/FamilyPresetData.cs` - Templates

Fluxo: FamilyPresetData (template) → FamilyManager (spawn/update) → Family (dados)

---

#### 🍔 Recursos (Comida, Água, etc)
Vá para:
- `Assets/Scripts/Resources/ResourceManager.cs` - Tracking e consumo
- `Assets/Scripts/Resources/ResourceType.cs` - Enum de tipos
- `Assets/Scripts/Resources/ResourceDefinition.cs` - Propriedades

Fluxo: ResourceManager parcelham consumo/produção

---

#### 🔔 Eventos (crises, alertas, etc)
Vá para:
- `Assets/Scripts/Events/EventManager.cs` - Trigger e resolução
- `Assets/Scripts/Events/EventData.cs` - Propriedades de evento
- `Assets/Scripts/Events/EventChoice.cs` - Opções do evento

Fluxo: EventManager.CheckTriggers() → mostra EventUIBridge

---

#### ⏱️ Tempo do jogo (dias, temperatura, estações)
Vá para:
- `Assets/Scripts/Managers/Survival/TimeSystem.cs` - Toda a lógica de tempo

Fluxo: Ticks → Dias → Estações → Temperatura

---

#### 💪 Morale (esperança da população)
Vá para:
- `Assets/Scripts/Systems/Survival/MoraleSystem.cs` - Cálculo de hope

Depende de: Recursos, população health, temperatura

---

#### 💀 Fome, Doença, Morte (Needs)
Vá para:
- `Assets/Scripts/Systems/Survival/NeedsSystem.cs` - Tudo aqui

Fluxo: TickContext → Fome/Sede → Saúde → Morte/Nascimento

---

#### 🎮 Input do jogador (mouse, teclado)
Vá para:
- `Assets/Scripts/Input/InputHandler.cs` - Parse input, raycast
- `Assets/Scripts/Input/CameraController.cs` - Câmera RTS

---

#### 🗺️ Grade (grid system)
Vá para:
- `Assets/Scripts/Grid/GridManager.cs` - Gerenciador principal
- `Assets/Scripts/Grid/GridCellData.cs` - Dados de célula
- `Assets/Scripts/Grid/GridCoordinate.cs` - Coordenadas X,Y

---

#### 🖼️ Preview visual (ghost buildings)
Vá para:
- `Assets/Scripts/Core/PreviewSystem.cs` - Gerencia preview

Usado por: BuildingSystem, RoadSystem

---

#### 🔙 Undo (desfazer placements)
Vá para:
- `Assets/Scripts/Core/PlacementHistory.cs` - Stack de ações

Usado por: BuildingSystem, RoadSystem

---

#### 🎨 Interface do usuário
Vá para:
- Menu principal: `Assets/Scripts/UI/MenuManager.cs`
- Top HUD: `Assets/Scripts/UI/TopHeaderHudController.cs`
- Build buttons: `Assets/Scripts/Managers/BuildHudController.cs`
- Bottom menu: `Assets/Scripts/UI/Bottom/BottomCategoryMenuController.cs`
- Tooltips: `Assets/Scripts/UI/Bottom/BuildingTooltipUI.cs`
- Eventos UI: `Assets/Scripts/UI/EventUIBridge.cs`

---

#### 📽️ Cenas (transições, loading, intro)
Vá para:
- Intro: `Assets/Scripts/Flow/IntroSequenceController.cs`
- Loading: `Assets/Scripts/Flow/LoadingSceneController.cs`
- Menu: `Assets/Scripts/UI/MenuManager.cs`
- Setup novo jogo: `Assets/Scripts/UI/NewGameSetupManager.cs`

---

## 🎯 Qual é o Ponto de Entrada?

**Se jogo está rodando...**

1. **Modo Build/Grid:**
   - `Managers/GameManager.cs` - Orquestra
   - Escuta: InputHandler, GridManager
   - Controla: BuildingSystem, RoadSystem

2. **Modo Survival:**
   - `Managers/Survival/GameManager.cs` - Orquestra
   - Coordena: TimeSystem, FamilyManager, ResourceManager, etc

3. **Cena inicial:**
   - `IntroSequenceController.cs` → `MenuManager.cs` → jogo

---

## 🔗 Padrões Comuns

### Pattern: Manager + System
```
Manager = Orquestra, coordena
System = Executa a lógica

Exemplo:
- GameManager (Survival) → coordena
- TimeSystem → executa ticks
- FamilyManager → gerencia famílias
- NeedsSystem → calcula fome/doença
```

### Pattern: Data + Logic Separation
```
Data = ScriptableObject com propriedades
Logic = Sistema que processa

Exemplo:
- BuildingData (template) + BuildingSystem (lógica)
- FamilyPresetData (template) + FamilyManager (lógica)
- EventData (template) + EventManager (lógica)
```

### Pattern: MonoBehaviour Instance
```
Template → System → Instância MonoBehaviour

Exemplo:
- Building.cs (instância no mundo)
- Road.cs (instância no mundo)
- Family.cs (dado serializado)
```

### Pattern: Event Callbacks
Sistema dispara eventos, UI escuta:
```
EventManager.OnEventTriggered
  └→ EventUIBridge escuta
  └→ UI mostra evento
```

---

## 📊 Estrutura de Pastas - Quick Navigation

```
Assets/Scripts/
│
├── Building/           → Edifícios
│   ├── BuildingSystem.cs
│   ├── Building.cs
│   └── BuildingData.cs
│
├── Core/               → Base do jogo
│   ├── GameMode.cs
│   ├── PlacementHistory.cs
│   ├── PreviewSystem.cs
│   └── Survival/
│       ├── GameStateSnapshot.cs
│       └── TickContext.cs
│
├── Events/             → Eventos dinâmicos
│   ├── EventManager.cs
│   ├── EventData.cs
│   └── EventChoice.cs
│
├── Families/           → População
│   ├── FamilyManager.cs
│   ├── Family.cs
│   ├── FamilyPresetData.cs
│   └── EmploymentStatus.cs
│
├── Grid/               → Sistema de grade
│   ├── GridManager.cs
│   ├── GridCellData.cs
│   ├── GridCoordinate.cs
│   └── OccupancyType.cs
│
├── Input/              → Controle do jogador
│   ├── InputHandler.cs
│   └── CameraController.cs
│
├── Managers/           → Orquestradores
│   ├── GameManager.cs (Build mode)
│   ├── BuildHudController.cs
│   └── Survival/
│       ├── GameManager.cs (Survival mode)
│       └── TimeSystem.cs
│
├── Resources/          → Comida, água, etc
│   ├── ResourceManager.cs
│   ├── ResourceType.cs
│   └── ResourceDefinition.cs
│
├── Road/               → Estradas
│   ├── RoadSystem.cs
│   └── Road.cs
│
├── Systems/            → Lógica de gameplay
│   └── Survival/
│       ├── NeedsSystem.cs (Fome/Doença)
│       └── MoraleSystem.cs (Esperança)
│
├── UI/                 → Interface
│   ├── MenuManager.cs
│   ├── TopHeaderHudController.cs
│   ├── EventUIBridge.cs
│   ├── NewGameSetupManager.cs
│   → NewGameSetupSelectionUI.cs
│   └── Bottom/
│       ├── BottomCategoryMenuController.cs
│       ├── BuildingMenuItemButton.cs
│       ├── RoadMenuItemButton.cs
│       ├── BuildingTooltipUI.cs
│       ├── BottomHudHoverLift.cs
│       └── BottomCategoryMenuGroup.cs
│
├── Flow/               → Transições de cena
│   ├── IntroSequenceController.cs
│   └── LoadingSceneController.cs
│
└── Editor/             → Editor only tools
    └── FixTerrainTrees.cs (Candidato a remover)
```

---

## 🚀 Checklist para Adicionar Nova Funcionalidade

### Adicionar novo tipo de recurso?
- [ ] `Resources/ResourceType.cs` - adicione enum
- [ ] `Resources/ResourceDefinition.cs` - crie ScriptableObject
- [ ] `Resources/ResourceManager.cs` - adicione tracking
- [ ] `UI/TopHeaderHudController.cs` - mostre no HUD

### Adicionar novo tipo de edifício?
- [ ] `Building/BuildingData.cs` - crie ScriptableObject
- [ ] Prefab do edifício (3D model + Building.cs component)
- [ ] `UI/Bottom/BuildingMenuItemButton.cs` - adicione botão

### Adicionar novo evento?
- [ ] `Events/EventData.cs` - crie ScriptableObject
- [ ] `Events/EventManager.cs` - adicione trigger logic
- [ ] `UI/EventUIBridge.cs` - verifique que mostra bem na UI

### Adicionar novo sistema de jogo?
- [ ] Crie novo arquivo em `Systems/Survival/` ou `Managers/Survival/`
- [ ] Faça ele escutar `TickContext` de `TimeSystem`
- [ ] Registre com `GameManager (Survival)`
- [ ] Dispatch eventos relevantes
- [ ] Update UI conforme necessário

---

## 🐛 Debugging

### "O jogo não startup"
Verificar:
1. `Flow/IntroSequenceController.cs` - existe na cena?
2. `UI/MenuManager.cs` - scene setup correto?
3. Verifique EditorScenes

### "Edifícios não colocam"
Verificar:
1. `Building/BuildingSystem.cs` - GetMode() == Build?
2. `Grid/GridManager.cs` - grid está inicializado?
3. `Input/InputHandler.cs` - raycast está funcionando?
4. `Core/PreviewSystem.cs` - preview aparece?

### "Recursos não consomem"
Verificar:
1. `Systems/Survival/NeedsSystem.cs` - tick está rodando?
2. `Resources/ResourceManager.cs` - initialized?
3. `Managers/Survival/GameManager.cs` - wiring correto?

### "UI não atualiza"
Verificar:
1. `UI/TopHeaderHudController.cs` - escuta OnStatChanged?
2. Sistema relevante (TimeSystem, ResourceManager) = dispatch eventos?

### "Eventos não triggeram"
Verificar:
1. `Events/EventManager.cs` - condições met?
2. `Events/EventData.cs` - cooldown venceu?
3. `UI/EventUIBridge.cs` - recebe callback?

---

## 💡 Dicas de Desenvolvimento

1. **Sempre escuta TickContext**
   - Se seu sistema roda a cada frame, use TimeSystem.Tick
   - Receba TickContext com tick number, day, temp

2. **Dispatch eventos, não chame direto**
   - Use `OnSomethingHappened` events
   - Deixe UI escutar

3. **Use ScriptableObjects para dados**
   - BuildingData, FamilyPresetData, EventData, etc
   - Reutilizável, editável no inspector

4. **GridManager é read-only para lógica**
   - Não modifique grid direto
   - Use BuildingSystem ou RoadSystem

5. **Teste a cada mudança**
   - Play test após modificar Manager ou System
   - Verifique UI atualiza corretamente

---

## 🎓 Recomendação de Leitura (na ordem)

Para entender o jogo melhor:

1. Leia `ARCHITECTURE_VISUAL_MAP.md` - Entenda o big picture
2. Leia `Flow/IntroSequenceController.cs` - Entrada do jogo
3. Leia `Managers/Survival/GameManager.cs` - Orquestra
4. Leia `Core/Survival/TickContext.cs` - Como ticks funcionam
5. Leia `Systems/Survival/NeedsSystem.cs` - Core gameplay logic
6. Leia `UI/TopHeaderHudController.cs` - Como UI update
7. Explore o resto conforme necessário!

---

**Dica Final:** Use o índice em `SCRIPT_ANALYSIS.md` como referência rápida! 🚀
