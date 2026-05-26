# Reclaim - GDD v1 Alignment Status

## Ajustes aplicados nesta rodada
- Added `Assets/Scripts/Managers/Survival/DayNightCycle.cs`
- Added `Assets/Scripts/Managers/AudioManager.cs`
- Added `Assets/Scripts/Managers/Survival/MusicLayer.cs`
- Added `Assets/Scripts/Events/EventBus.cs`
- Updated `Assets/Scripts/Managers/Survival/GameManager.cs`
  - Regras de derrota alinhadas ao GDD 15.1:
  - Populacao total zero
  - Nenhuma familia restante
  - Moral media <= limiar por N dias consecutivos (configuravel)

## Milestones do GDD (visao rapida)
- M0 Grid/Input/Camera: Parcialmente pronto
- M1 Building (3 estagios + progress UI + undo): Pronto no core
- M2 Floresta (spawn/harvest/regrow): Pronto no core
- M3 Familias/Needs/Happiness: Pronto (nome atual `MoraleSystem`)
- M4 Trabalho/Producao: Parcial (faltam scripts dedicados de workplace conforme tabela do GDD)
- M5 Dia/Noite/Eventos: Parcial (DayNightCycle criado; integrar UI de velocidade/fase)
- M6 Defesa/Hordas: Pendente
- M7 Expedicoes: Pendente
- M8 Pesquisa/Tech: Pendente
- M9 Save/Audio/VFX polish: Parcial (audio base criada)

## Observacoes
- `EventBus.cs` foi adicionado para desacoplamento conforme cap. 11 do GDD.
- Recomenda-se migrar eventos diretos entre sistemas para `EventBus` aos poucos para evitar regressao.
