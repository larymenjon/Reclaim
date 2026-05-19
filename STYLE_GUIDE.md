# Codex Interno: Diretrizes de Clean Code e Boas Práticas (C# / Unity)

Este documento define o padrão oficial para todos os scripts do projeto **RECLAIM**.  
Objetivo: manter código simples, legível, testável e sustentável no longo prazo.

## Tecnologias de Preferência

- `C#`
- `Unity`

---

## 1. Princípios Gerais

### KISS (Keep It Simple, Stupid)
- Prefira a solução mais simples que resolve o problema.
- Evite abstrações prematuras e arquiteturas complexas sem necessidade real.

### DRY (Don't Repeat Yourself)
- Não duplique regra de negócio.
- Se lógica semelhante aparece em mais de um lugar, extraia para método, componente ou serviço reutilizável.

### YAGNI (You Ain't Gonna Need It)
- Não implemente “previsões de futuro”.
- Só adicione código quando houver requisito real e atual.

---

## 2. Nomenclatura (Naming Conventions)

### Regras gerais
- Use nomes descritivos e intencionais.
- Evite abreviações confusas (`tmp`, `mgr`, `cfg`) quando houver alternativa clara.
- Nomes devem explicar **o que é** ou **o que faz**.

### Padrão por tipo (C# / Unity)
- Classes, structs, interfaces, enums, métodos, propriedades: `PascalCase`
- Variáveis locais e parâmetros: `camelCase`
- Campos privados: `_camelCase`
- Constantes: `PascalCase` (ou `UPPER_SNAKE_CASE` apenas se já for padrão consolidado da equipe)
- Interface: prefixo `I` (`IInventoryService`)
- Eventos: nome com verbo no passado ou ação clara (`OnHealthChanged`)

### Exemplo

```csharp
// Antes (Ruim)
int hp;
void calc() { }

// Depois (Bom)
int currentHealth;
void CalculateDamageOutput() { }
```

---

## 3. Funções e Métodos

### Regras
- Um método deve ter **responsabilidade única**.
- Idealmente curto (referência: até ~20 linhas; exceções justificadas).
- Prefira até **3 parâmetros**. Acima disso, considere objeto de parâmetros.
- Evite efeitos colaterais escondidos.
- Retornos devem ser previsíveis e explícitos.

### Exemplo: método fazendo coisas demais

```csharp
// Antes (Ruim)
void ProcessTurn(Player player)
{
    // valida estado
    // aplica dano
    // toca som
    // atualiza HUD
    // salva progresso
}

// Depois (Bom)
void ProcessTurn(Player player)
{
    if (!CanProcessTurn(player)) return;

    ApplyTurnDamage(player);
    PlayTurnFeedback();
    UpdateTurnUI(player);
    SaveTurnState(player);
}
```

---

## 4. Comentários vs. Código Autoexplicativo

### Quando comentar
- Para explicar **por que** uma decisão foi tomada.
- Para documentar regra de negócio não óbvia.
- Para alertar sobre comportamento de engine/API que pode confundir.

### Quando evitar comentário
- Quando o comentário só repete o que o código já diz.
- Se precisa comentar demais para entender, o código deve ser refatorado.

### Exemplo

```csharp
// Antes (Ruim)
// Incrementa i em 1
i++;

// Depois (Bom)
// Unity pode disparar múltiplos eventos no mesmo frame em devices lentos.
// Este guard evita processamento duplicado.
if (_lastProcessedFrame == Time.frameCount) return;
_lastProcessedFrame = Time.frameCount;
```

---

## 5. Tratamento de Erros

### Regras
- Nunca usar `catch` vazio.
- Faça logs com contexto suficiente (entidade, id, estado, ação).
- Use níveis corretos:
  - `Debug.Log` para informação de desenvolvimento
  - `Debug.LogWarning` para comportamento inesperado recuperável
  - `Debug.LogError` para falhas relevantes
- Lance exceções quando o estado for inválido e não recuperável naquele contexto.
- Não use exceção para controle de fluxo comum.

### Exemplo

```csharp
// Antes (Ruim)
try
{
    SaveGame();
}
catch
{
}

// Depois (Bom)
try
{
    SaveGame();
}
catch (IOException ex)
{
    Debug.LogError($"Falha ao salvar jogo. Slot={slotId}, Player={playerId}. Erro={ex.Message}");
    throw;
}
```

---

## 6. Formatação e Organização Visual

### Regras
- Indentação: 4 espaços (sem tabs).
- Uma instrução por linha.
- Linha ideal: até 100-120 caracteres.
- Um arquivo por classe principal.
- Ordem recomendada em classe:
  1. Constantes
  2. Campos serializados (`[SerializeField]`)
  3. Campos privados
  4. Propriedades
  5. Eventos
  6. Métodos Unity (`Awake`, `Start`, `Update`...)
  7. Métodos públicos
  8. Métodos privados
- Use linhas em branco para separar blocos lógicos.
- Remova código morto e comentários obsoletos.

### Exemplo

```csharp
// Antes (Ruim)
public class PlayerController:MonoBehaviour{
[SerializeField]float speed;int hp;void Update(){Move();if(hp<=0){Die();}}void Move(){/*...*/}void Die(){/*...*/}}

// Depois (Bom)
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    private int currentHealth;

    private void Update()
    {
        Move();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Move()
    {
        // ...
    }

    private void Die()
    {
        // ...
    }
}
```

---

## 7. Checklist Rápido (Antes de Commitar)

- O código está simples (KISS)?
- Evitei repetição desnecessária (DRY)?
- Evitei features sem requisito real (YAGNI)?
- Nomes estão claros e consistentes?
- Métodos têm uma responsabilidade?
- Tratamento de erro está explícito e sem `catch` vazio?
- Formatação está consistente?
- Removi código/comentários obsoletos?

---

## 8. Como Usar Este Codex no Dia a Dia

- Use este arquivo como referência obrigatória para qualquer script novo.
- Em PR/Code Review, valide mudanças contra este guia.
- Para revisão assistida por IA, use:

```text
Revise este código com base no Codex de Clean Code que criamos anteriormente.
```

---

## 9. Política do Projeto RECLAIM

Todo código novo deve seguir este padrão.  
Exceções são permitidas apenas quando:
- houver limitação técnica comprovada;
- o trade-off estiver documentado no PR/commit.

