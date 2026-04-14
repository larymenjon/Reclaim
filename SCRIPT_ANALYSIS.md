# 📋 Análise Completa de Scripts - Projeto Reclaim

**Data da Análise:** 14 de Abril, 2026  
**Total de Scripts:** 47 arquivos  
**Organização:** 13 diretórios principais

---

## 🎯 MAPA DO PROJETO - ONDE CADA SCRIPT VAI

### **NÚCLEO DO JOGO (Core)**
Estes scripts formam a base essencial do jogo.

| Script | Função | Status |
|--------|--------|--------|
| `Core/GameMode.cs` | Enum com modos de jogo (Build, Road, Demolish) | ✅ Essencial |
| `Core/PlacementHistory.cs` | Sistema de desfazer (undo) para placements | ✅ Essencial |
| `Core/PreviewSystem.cs` | Preview visual (ghost) ao colocar estruturas | ✅ Essencial |
| `Core/Survival/GameStateSnapshot.cs` | Snapshot read-only do estado do jogo | ✅ Essencial |
| `Core/Survival/TickContext.cs` | Contexto passado a cada tick do jogo | ✅ Essencial |

**Recomendação:** Manter todos - formam a base do sistema.

---

### **SISTEMA DE GRADE (Grid System)**
Gerencia o mapa em grid onde tudo é colocado.

| Script | Função | Status |
|--------|--------|--------|
| `Grid/GridManager.cs` | Gerenciador principal - dimensões, células, ocupação | ✅ Essencial |
| `Grid/GridCellData.cs` | Dados de uma célula individual | ✅ Essencial |
| `Grid/GridCoordinate.cs` | Struct de coordenadas X,Y | ✅ Essencial |
| `Grid/OccupancyType.cs` | Enum (None/Building/Road) | ✅ Essencial |

**Recomendação:** Manter todos - críticos para o sistema.

---

### **SISTEMA DE CONSTRUÇÃO (Building System)**
Gerencia colocação e gerenciamento de edifícios.

| Script | Função | Status |
|--------|--------|--------|
| `Building/BuildingSystem.cs` | Lógica de seleção, validação e placement | ✅ Essencial |
| `Building/Building.cs` | Componente MonoBehaviour de um edifício colocado | ✅ Essencial |
| `Building/BuildingData.cs` | ScriptableObject com propriedades do edifício | ✅ Essencial |

**Recomendação:** Manter todos - sistema completo e necessário.

---

### **SISTEMA DE ESTRADAS (Road System)**
Gerencia criação de estradas.

| Script | Função | Status |
|--------|--------|--------|
| `Road/RoadSystem.cs` | Lógica drag-based de placement de estradas | ✅ Essencial |
| `Road/Road.cs` | Componente de estrada colocada (conexões N/E/S/W) | ✅ Essencial |

**Recomendação:** Manter ambos - sistema completo.

---

### **SISTEMA DE ENTRADA (Input System)**
Gerencia controle do jogo.

| Script | Função | Status |
|--------|--------|--------|
| `Input/InputHandler.cs` | Converte input raw em eventos de grid | ✅ Essencial |
| `Input/CameraController.cs` | Câmera RTS (pan, zoom, rotação) | ✅ Essencial |

**Recomendação:** Manter ambos - controle completo.

---

### **SISTEMA DE SOBREVIVÊNCIA (Survival - Modo principal)**
Toda a lógica de gameplay do modo de sobrevivência.

#### **Famílias & População**
| Script | Função | Status |
|--------|--------|--------|
| `Families/Family.cs` | Classe serializada representando uma família | ✅ Essencial |
| `Families/FamilyManager.cs` | Gerencia ciclo de vida das famílias | ✅ Essencial |
| `Families/FamilyPresetData.cs` | ScriptableObject com configurações de famílias | ✅ Essencial |
| `Families/EmploymentStatus.cs` | Enum (Unemployed/Employed/CriticalWorker) | ✅ Essencial |

#### **Recursos**
| Script | Função | Status |
|--------|--------|--------|
| `Resources/ResourceManager.cs` | Gerencia Food, Water, Materials, Energy | ✅ Essencial |
| `Resources/ResourceDefinition.cs` | ScriptableObject de um tipo de recurso | ✅ Essencial |
| `Resources/ResourceType.cs` | Enum dos tipos de recursos | ✅ Essencial |

#### **Eventos Dinâmicos**
| Script | Função | Status |
|--------|--------|--------|
| `Events/EventManager.cs` | Triggers eventos baseado em condições | ✅ Essencial |
| `Events/EventData.cs` | ScriptableObject de um evento | ✅ Essencial |
| `Events/EventChoice.cs` | Struct com opções de escolha do evento | ✅ Essencial |

#### **Sistemas de Jogo**
| Script | Função | Status |
|--------|--------|--------|
| `Managers/Survival/TimeSystem.cs` | Simula tempo do jogo, temperatura, dia/noite | ✅ Essencial |
| `Systems/Survival/NeedsSystem.cs` | Fome/sede, doença, morte, nascimento | ✅ Essencial |
| `Systems/Survival/MoraleSystem.cs` | Calcula morale baseado em recursos/saúde | ✅ Essencial |
| `Managers/Survival/GameManager.cs` | Coordenador dos sistemas de sobrevivência | ✅ Essencial |

#### **Bootstrap & Inicialização**
| Script | Função | Status |
|--------|--------|--------|
| `Managers/Survival/SurvivalRuntimeBootstrap.cs` | Auto-instancia sistemas se não estiverem na cena | ⚠️ **VERIFICAR** |

**Recomendação:** `SurvivalRuntimeBootstrap.cs` pode ser problemático - instancia coisas automaticamente sem controle do designer. Considere remover e usar Scene Setup properly.

---

### **INTERFACE DE USUÁRIO (UI)**
Sistema completo de menus e HUD.

#### **Menus Principais**
| Script | Função | Status |
|--------|--------|--------|
| `UI/MenuManager.cs` | Menu principal - navegação entre cenas | ✅ Essencial |
| `UI/NewGameSetupManager.cs` | Setup scene (líder/mapa/dificuldade) | ✅ Essencial |
| `UI/NewGameSetupSelectionUI.cs` | UI controller de seleção | ✅ Essencial |

#### **HUD in-game**
| Script | Função | Status |
|--------|--------|--------|
| `UI/TopHeaderHudController.cs` | HUD topo - stats (pop, recursos, casas) | ✅ Essencial |
| `UI/BuildHudController.cs` | HUD - botões de construção/estrada | ✅ Essencial |

#### **Bottom HUD (Menu de Categorias)**
| Script | Função | Status |
|--------|--------|--------|
| `UI/Bottom/BottomCategoryMenuController.cs` | Gerenciador de abas inferior | ✅ Essencial |
| `UI/Bottom/BottomCategoryMenuGroup.cs` | Auto-wire de grupos de categorias | ⚠️ **REDUNDANTE?** |
| `UI/Bottom/BottomHudHoverLift.cs` | Animação de hover (levanta UI) | ✅ Essencial |
| `UI/Bottom/BuildingMenuItemButton.cs` | Botão de edifício com tooltip | ✅ Essencial |
| `UI/Bottom/RoadMenuItemButton.cs` | Botão de estrada | ✅ Essencial |
| `UI/Bottom/BuildingTooltipUI.cs` | Tooltip dinâmico de edifícios | ✅ Essencial |

#### **Eventos UI**
| Script | Função | Status |
|--------|--------|--------|
| `UI/EventUIBridge.cs` | Bridge entre EventManager e UI | ✅ Essencial |

---

### **FLUXO DE CENAS (Scene Flow)**
Controla transições e loading.

| Script | Função | Status |
|--------|--------|--------|
| `Flow/IntroSequenceController.cs` | Logos, vídeo, splash screen | ✅ Essencial |
| `Flow/LoadingSceneController.cs` | Loading screen com async loading | ✅ Essencial |

**Recomendação:** Manter ambos, mas verificar se `IntroSequenceController` está sendo usado.

---

### **EDITORS & UTILITIES**
Ferramentas de desenvolvimento.

| Script | Função | Status |
|--------|--------|--------|
| `Editor/FixTerrainTrees.cs` | Menu editor para limpar tree prefabs | ⚠️ **VERIFICAR** |

**Recomendação:** Este é apenas um utilitário editor - pode ser removido se a issue de árvores já foi resolvida.

---

### **GERENCIADORES PRINCIPAIS (Entry Points)**
Scripts que orquestram tudo.

| Script | Função | Status |
|--------|--------|--------|
| `Managers/GameManager.cs` | Composição root - wire de sistemas principais | ✅ Essencial |
| `Managers/Survival/GameManager.cs` | Coordenador modo sobrevivência | ✅ Essencial |

⚠️ **PROBLEMA IDENTIFICADO:** Dois `GameManager.cs` em locais diferentes!
- `Managers/GameManager.cs` - parece ser para modo construção/principal
- `Managers/Survival/GameManager.cs` - para modo sobrevivência

Isso pode causar confusão. Recomenda-se renomear um deles para clareza.

---

## 🔍 ANÁLISE DE REDUNDÂNCIAS & PROBLEMAS

### ⚠️ Potenciais Problemas Identificados:

1. **Dois GameManager.cs**
   - **Localização:** `Managers/GameManager.cs` e `Managers/Survival/GameManager.cs`
   - **Problema:** Pode causar confusão qual é qual
   - **Solução:** Renomear `Managers/GameManager.cs` para `Managers/BuildGameManager.cs` ou mover para estrutura mais clara

2. **SurvivalRuntimeBootstrap.cs**
   - **Problema:** Auto-instancia sistemas, removes controle do designer
   - **Solução:** Remover e usar Scene Setup adequado ou DI container

3. **BottomCategoryMenuGroup.cs**
   - **Problema:** Auto-wire automático - pode ser simplificado
   - **Solução:** Integrar lógica diretamente em `BottomCategoryMenuController` ou tornador explícito

---

## ✨ PADRÕES BEM IMPLEMENTADOS

✅ **Excelente separação de concerns:**
- Core logic separada de UI
- Sistemas bem desacoplados
- ScriptableObjects para dados

✅ **Estrutura de diretórios clara:**
- Agrupa por feature/domínio
- Fácil de navegar
- Escalável

✅ **Padrão Manager + Data:**
- BuildingSystem + Building + BuildingData
- RoadSystem + Road
- FamilyManager + Family + FamilyPresetData
- Padrão consistente e previsível

---

## 📊 CONTAGEM POR CATEGORIA

| Categoria | Scripts | Essencial | Remover? |
|-----------|---------|-----------|----------|
| Core | 5 | 5 | Não |
| Grid | 4 | 4 | Não |
| Building | 3 | 3 | Não |
| Road | 2 | 2 | Não |
| Input | 2 | 2 | Não |
| Families | 4 | 4 | Não |
| Resources | 3 | 3 | Não |
| Events | 3 | 3 | Não |
| Survival Systems | 3 | 3 | Não |
| Survival Managers | 2 | 2 | Revisar |
| UI | 11 | 10 | 1 possível |
| Flow | 2 | 1-2 | Revisar |
| Editor | 1 | 0 | Sim (utilidade) |
| **TOTAL** | **47** | **44** | **3 para revisar** |

---

## 🎬 FLUXO DO JOGO - COMO TUDO FUNCIONA

### **Startup (Fluxo de Cenas)**
```
1. IntroSequenceController
   ↓ (Logo, vídeo, splash)
2. MenuManager (Menu Principal)
   ↓ (Novo Jogo / Carregar)
3. NewGameSetupManager
   ↓ (Setup de líder, mapa, dificuldade)
4. LoadingSceneController
   ↓ (Loading screen async)
5. GameScene + GameManager
```

### **GameScene Runtime (Modo Sobrevivência)**
```
GameManager (Survival)
├── TimeSystem → ticks do jogo, temperatura, dia/noite
├── FamilyManager → populacional, emprego, saúde
├── ResourceManager → Food, Water, Materials, Energy
│   ├── NeedsSystem → consumo, fome, doença
│   └── MoraleSystem → cálculo de esperança
└── EventManager → eventos dinâmicos
    └── EventUIBridge → UI de eventos

Grid System (Paralelo)
├── GridManager → gerencia mapa em grid
├── BuildingSystem → colocar edifícios
│   ├── PreviewSystem → ghost preview
│   └── Building.cs (instâncias)
└── RoadSystem → colocar estradas
    └── Road.cs (instâncias)

Input System (Contínuo)
├── InputHandler → mouse/teclado → grid events
├── CameraController → câmera RTS
└── PlacementHistory → undo system
```

### **UI Layer (Sempre ativo)**
```
MenuManager (Main Menu)
├── BuildHudController → botões build/road
├── TopHeaderHudController → stats top
└── BottomCategoryMenuController
    ├── BuildingMenuItemButton → UI building items
    └── RoadMenuItemButton → UI road
        └── BuildingTooltipUI → tooltips

EventUIBridge → mostra eventos dinâmicos
```

---

## 🚀 RECOMENDAÇÕES DE LIMPEZA

### **Remove (Seguro)**
1. ✅ `Editor/FixTerrainTrees.cs` - Apenas utilidade Editor, não crítico para jogo

### **Considere Remover (Depende de Uso)**
2. ⚠️ `Managers/Survival/SurvivalRuntimeBootstrap.cs` - Auto-instantiation pode substituir arquitetura apropriada

### **Refatore (Melhore Arquitetura)**
3. 🔧 Renomear `Managers/GameManager.cs` → `Managers/BuildGameManager.cs` (clareza)
4. 🔧 Integrar `BottomCategoryMenuGroup.cs` diretamente ou tornar explícito (reduzir magic auto-wiring)
5. 🔧 Considerar remover `SurvivalRuntimeBootstrap` e usar cena bem estruturada

### **Documentação**
6. 📝 Adicionar comentários XML aos GameManagers explicando sua responsabilidade
7. 📝 Documentar estrutura de eventos em EventManager

---

## 📌 ESTRUTURA RECOMENDADA PÓS-LIMPEZA

```
Assets/Scripts/
├── Core/                    # Sistemas base
│   ├── GameMode.cs
│   ├── PlacementHistory.cs
│   ├── PreviewSystem.cs
│   └── Survival/
│       ├── GameStateSnapshot.cs
│       └── TickContext.cs
│
├── Grid/                    # Sistema deGrid
│   ├── GridManager.cs
│   ├── GridCellData.cs
│   ├── GridCoordinate.cs
│   └── OccupancyType.cs
│
├── Placement/               # Building + Road (pode consolidar)
│   ├── Building/
│   │   ├── BuildingSystem.cs
│   │   ├── Building.cs
│   │   └── BuildingData.cs
│   └── Road/
│       ├── RoadSystem.cs
│       └── Road.cs
│
├── Simulation/              # Survival mechanics
│   ├── Families/
│   ├── Resources/
│   ├── Events/
│   └── Systems/
│       └── Survival/
│           ├── TimeSystem.cs
│           ├── NeedsSystem.cs
│           └── MoraleSystem.cs
│
├── Input/                   # Player input
│   ├── InputHandler.cs
│   └── CameraController.cs
│
├── Managers/                # Orquestradores
│   ├── BuildGameManager.cs  # (renomeado)
│   └── SurvivalGameManager.cs
│
├── UI/                      # Interface
│   ├── Menus/
│   ├── HUD/
│   └── Components/
│
├── Flow/                    # Scene transitions
│   ├── IntroSequenceController.cs
│   └── LoadingSceneController.cs
│
└── Editor/                  # (Remover se não necessário)
```

---

## ✅ CONCLUSÃO

**Status Geral:** 🟢 **BOM**

Seu projeto está bem estruturado! A maioria dos scripts são essenciais e bem organizados. 

**Próximos Passos:**
1. Remover `FixTerrainTrees.cs` (utilidade editor)
2. Considerar remover `SurvivalRuntimeBootstrap.cs` (auto-magic ruim)
3. Renomear GameManagers para clareza
4. Adicionar documentação XML aosGameManagers
5. Adicionar comentário em cada System explicando o que faz

**Resultado esperado:** Código mais limpo, legível, e fácil de manter! 🎮
