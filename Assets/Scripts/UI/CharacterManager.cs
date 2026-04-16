using System;
using UnityEngine;

namespace Reclaim.UI
{
    [CreateAssetMenu(fileName = "CharacterManager", menuName = "Reclaim/Character Manager")]
    public class CharacterManager : ScriptableObject
    {
        public CharacterData[] characters;

        [Serializable]
        public class CharacterData
        {
            [Header("Informações Básicas")]
            public string characterName;
            public string description;
            public string quote;
            public Sprite portrait;
            
            [Header("Especialização")]
            public CharacterSpecialization specialization;
            
            [Header("Habilidades")]
            public string passiveName;
            public string passiveDescription;
            public string activeName;
            public string activeDescription;
            
            [Header("Buffs")]
            [Range(-50f, 50f)] public float constructionSpeedBonus;
            [Range(-50f, 50f)] public float materialCostBonus;
            [Range(-50f, 50f)] public float defenseBonus;
            [Range(-50f, 50f)] public float happinessBonus;
            [Range(-50f, 50f)] public float foodProductionBonus;
            [Range(-50f, 50f)] public float medicalEfficiencyBonus;
            [Range(-50f, 50f)] public float explorationSpeedBonus;
            [Range(-50f, 50f)] public float resourceBonus;
            
            [Header("Debuffs")]
            [Range(-50f, 50f)] public float revoltChanceBonus;
            [Range(-50f, 50f)] public float resourceConsumptionBonus;
            [Range(-50f, 50f)] public float expansionBonus;
            [Range(-50f, 50f)] public float securityBonus;
            [Range(-50f, 50f)] public float productivityBonus;
            [Range(-50f, 50f)] public float trustBonus;
            
            [Header("Evento Especial")]
            public string eventName;
            public string eventDescription;
        }

        public enum CharacterSpecialization
        {
            Construction,
            Defense,
            Food,
            Health,
            Exploration,
            Resources,
            Morale
        }

        private void OnEnable()
        {
            InitializeCharacters();
        }

        private void InitializeCharacters()
        {
            characters = new CharacterData[]
            {
                // Helena Kravik - A ENGENHEIRA PRAGMÁTICA
                new CharacterData
                {
                    characterName = "Helena Kravik",
                    description = "Helena era responsável por megaprojetos antes do colapso. Hoje, ela reconstrói o mundo com o que sobrou — sem espaço para erros ou sentimentalismo.",
                    quote = "Estruturas não falham. Pessoas falham.",
                    specialization = CharacterSpecialization.Construction,
                    passiveName = "Eficiência Brutal",
                    passiveDescription = "Construções são concluídas mais rápido.",
                    activeName = "Improvisação Estrutural",
                    activeDescription = "Permite construir sem todos os recursos necessários (qualidade reduzida).",
                    constructionSpeedBonus = 25f,
                    materialCostBonus = -20f,
                    happinessBonus = -15f,
                    productivityBonus = -15f,
                    eventName = "Cortar Custos",
                    eventDescription = "Usar materiais inferiores para acelerar expansão?"
                },

                // Marcus Hale - O EX-MILITAR VETERANO
                new CharacterData
                {
                    characterName = "Marcus Hale",
                    description = "Veterano de guerras esquecidas, Marcus acredita que disciplina é a única coisa que separa ordem do caos.",
                    quote = "Segurança não é opcional. É sobrevivência.",
                    specialization = CharacterSpecialization.Defense,
                    passiveName = "Doutrina de Combate",
                    passiveDescription = "Unidades defensivas mais eficientes.",
                    activeName = "Lei Marcial",
                    activeDescription = "Reduz revoltas drasticamente por um tempo.",
                    defenseBonus = 30f,
                    revoltChanceBonus = -20f,
                    happinessBonus = -20f,
                    resourceConsumptionBonus = 10f,
                    eventName = "Toque de Recolher",
                    eventDescription = "Impor controle total da população?"
                },

                // Elara Venn - A BOTÂNICA IDEALISTA
                new CharacterData
                {
                    characterName = "Elara Venn",
                    description = "Mesmo após o fim, Elara acredita que a vida sempre encontra um caminho. Ela luta para reconstruir um mundo sustentável.",
                    quote = "Se a terra ainda respira… nós também podemos.",
                    specialization = CharacterSpecialization.Food,
                    passiveName = "Cultivo Sustentável",
                    passiveDescription = "Produção de comida aumentada.",
                    activeName = "Solo Regenerativo",
                    activeDescription = "Recupera áreas contaminadas lentamente.",
                    foodProductionBonus = 30f,
                    happinessBonus = 10f,
                    expansionBonus = -15f,
                    securityBonus = -10f,
                    eventName = "Fertilizante Proibido",
                    eventDescription = "Usar químicos para aumentar produção?"
                },

                // Dr. Adrian Voss - O MÉDICO DESILUDIDO
                new CharacterData
                {
                    characterName = "Dr. Adrian Voss",
                    description = "Ele já salvou milhares. Agora, precisa escolher quem ainda vale a pena salvar.",
                    quote = "Não posso salvar todos… então escolho quem vive.",
                    specialization = CharacterSpecialization.Health,
                    passiveName = "Triagem Rigorosa",
                    passiveDescription = "Cura mais eficiente com menos recursos.",
                    activeName = "Decisão Clínica",
                    activeDescription = "Salva parte da população em situações críticas.",
                    medicalEfficiencyBonus = 35f,
                    resourceBonus = -20f,
                    happinessBonus = -15f,
                    eventName = "Quem Vive?",
                    eventDescription = "Recursos insuficientes para todos."
                },

                // Kael Ryn - A EXPLORADORA NÔMADE
                new CharacterData
                {
                    characterName = "Kael Ryn",
                    description = "Kael viveu fora dos muros por anos. O desconhecido não a assusta — é onde ela se sente viva.",
                    quote = "O mundo acabou… mas nunca foi tão aberto.",
                    specialization = CharacterSpecialization.Exploration,
                    passiveName = "Caminhos Ocultos",
                    passiveDescription = "Descobre áreas secretas.",
                    activeName = "Expedição Rápida",
                    activeDescription = "Reduz tempo de exploração drasticamente.",
                    explorationSpeedBonus = 30f,
                    resourceBonus = 20f,
                    securityBonus = -15f,
                    eventName = "Zona Mortal",
                    eventDescription = "Explorar área com altíssimo risco?"
                },

                // Drex Morrow - O NEGOCIANTE SCAVENGER
                new CharacterData
                {
                    characterName = "Drex Morrow",
                    description = "Drex nunca confiou em ninguém — e sobreviveu por causa disso. Ele vê valor onde os outros veem lixo.",
                    quote = "Tudo tem preço. Inclusive você.",
                    specialization = CharacterSpecialization.Resources,
                    passiveName = "Barganha Suja",
                    passiveDescription = "Trocas mais vantajosas.",
                    activeName = "Estoque Oculto",
                    activeDescription = "Gera recursos extras temporariamente.",
                    resourceBonus = 25f,
                    trustBonus = -15f,
                    eventName = "Negócio Suspeito",
                    eventDescription = "Aceitar troca arriscada?"
                },

                // Mara Solis - A LÍDER COMUNITÁRIA
                new CharacterData
                {
                    characterName = "Mara Solis",
                    description = "Mara mantém as pessoas unidas quando tudo desmorona. Ela entende que esperança também é um recurso.",
                    quote = "Sem pessoas… não existe cidade.",
                    specialization = CharacterSpecialization.Morale,
                    passiveName = "União Popular",
                    passiveDescription = "Aumenta moral geral.",
                    activeName = "Festival da Esperança",
                    activeDescription = "Remove penalidades temporariamente.",
                    happinessBonus = 30f,
                    revoltChanceBonus = -20f,
                    productivityBonus = -10f,
                    resourceConsumptionBonus = 10f,
                    eventName = "Última Celebração",
                    eventDescription = "Gastar recursos para manter esperança?"
                }
            };
        }

        public CharacterData GetCharacter(int index)
        {
            if (index >= 0 && index < characters.Length)
            {
                return characters[index];
            }
            return null;
        }

        public int GetCharacterCount()
        {
            return characters.Length;
        }
    }
}