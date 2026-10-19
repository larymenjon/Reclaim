<p align="center">
  <img src="https://i.ibb.co/Y44TKpZ4/29727b9c-3a37-4d24-96f3-89b9d39f9af0-1.png" alt="RECLAIM" style="max-width: 100%; height: auto;"/>
</p>

<h3 align="center">Reconstrua o mundo. Pague o preço.</h3>

<p align="center">
  <img src="https://img.shields.io/badge/Unity-6.6-black?style=for-the-badge&logo=unity"/>
  <img src="https://img.shields.io/badge/C%23-9-purple?style=for-the-badge&logo=csharp"/>
  <img src="https://img.shields.io/badge/Status-Alpha-yellow?style=for-the-badge"/>
</p>

RECLAIM é um city builder de sobrevivência pós-apocalíptico em que cada escolha tem custo humano. Você lidera famílias sobreviventes, constrói a colônia em um grid 3D e tenta atravessar o inverno com comida, água e esperança contadas em dias.

## Estado atual (alpha)

Funciona hoje: construção em grid com estágios de obra e desfazer, estradas com curva, floresta com corte automático, famílias com fome, saúde, moral, doença, nascimento e morte, clima por estação, esperança coletiva, diretor de eventos com dilemas, fluxo de Intro, Menu, Novo Jogo e Loading.

HUD do redesign do Figma: montada pelo menu **Reclaim > Criar HUD do redesign** (detalhes no GDD, seção 14.1).

Em desenvolvimento: telas de Novo Jogo do redesign, moradia e aquecimento, produção com poço e horta, conteúdo de eventos.

O plano completo está no [GDD](docs/GDD.md).

## Como abrir o projeto

1. Instale o [Git LFS](https://git-lfs.com) e rode `git lfs install`. Cenas e modelos ficam no LFS.
2. Clone: `git clone https://github.com/larymenjon/reclaim.git`
3. Abra no Unity Hub com a versão **6000.6.x**.
4. Abra `Assets/Scenes/Game.unity` e use o menu **Reclaim > Configurar cena de jogo** se a cena ainda não tiver o objeto "Simulação". Depois, **Reclaim > Criar HUD do redesign**.
5. Para jogar o fluxo completo, comece pela cena `Intro`.

Evite deixar o projeto dentro do OneDrive ou de outra pasta sincronizada: a sincronização trava arquivos da pasta Library.

## Estrutura

```
Assets/
  Scripts/       código, um namespace por pasta (ver GDD, seção 11)
  Data/Config/   recursos, clima e famílias iniciais em ScriptableObjects
  Data/          líderes e construções
  Prefabs/  Scenes/  UI/  Imports/
docs/GDD.md      design do jogo
STYLE_GUIDE.md   padrão de código
```

## Equipe

Laryssa Aguiar Menjon: game design, programação e UI/UX.
