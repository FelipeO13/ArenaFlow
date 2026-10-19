# 🥊 ArenaFlow - Sistema de Gestão para Academias e Centros de Luta

Sistema completo de gerenciamento para academias e centros de treinamento de boxe e artes marciais (**Nobre Arte Boxe Club**), desenvolvido em **C# / .NET 8 (ASP.NET Core)** com **Entity Framework Core** e banco de dados **SQLite**.

---

## 🚀 Como Executar o Sistema

### Opção 1: Execução Rápida (1 Clique)
Basta dar um duplo clique no arquivo [`iniciar.bat`](file:///C:/Users/olive/.gemini/antigravity/scratch/BoxeGymManager/iniciar.bat) localizado na raiz do projeto. Ele abrirá o navegador diretamente em:
👉 **[http://localhost:5000](http://localhost:5000)**

### Opção 2: Pelo Terminal / PowerShell
```powershell
cd C:\Users\olive\.gemini\antigravity\scratch\BoxeGymManager
dotnet run --urls "http://localhost:5000"
```

---

## 🥊 Funcionalidades Implementadas

### 1. 📋 Gestão Completa de Alunos e Atletas (`/Students`)
- **Ficha Cadastral**: Nome, Matrícula automática (ex: `BX-1001`), CPF, data de nascimento, telefone/WhatsApp, e-mail e endereço.
- **Contato de Emergência**: Nome e telefone de emergência para rápida assistência em caso de lesão nos treinos.
- **Ficha Técnica de Boxe**:
  - Categoria de peso oficial (Peso Mosca, Pena, Leve, Meio-Médio, Médio, Meio-Pesado, Pesado, etc.).
  - Nível esportivo (Iniciante, Amador, Intermediário, Sparring, Competidor Profissional).
  - Base de luta (Destro / Orthodox, Canhoto / Southpaw, Switch).
  - Controle de atestado médico e restrições cardiovasculares/articulares.
- **Perfil Individual**: Histórico de frequência no ringue e extrato de mensalidades com 1 clique.

### 2. 🚪 Controle de Entrada & Saída / Catraca (`/CheckIn`)
- **Portaria Rápida**: Identificação por código de matrícula (`BX-1001`), CPF ou nome.
- **Validação Automática em Tempo Real**:
  - 🟢 **Liberado**: Aluno ativo e com mensalidade em dia.
  - 🟡 **Alerta de Vencimento**: Acesso com aviso se houver pagamento atrasado ou plano expirado.
  - 🔴 **Bloqueado**: Aluno com matrícula inativa ou suspensa.
- **Controle de Ocupação**: Visualização em tempo real de quem está dentro da academia e botão de registro de saída.

### 3. 💳 Controle Financeiro & Mensalidades (`/Payments`)
- Registro de pagamentos por **Pix**, **Cartão de Crédito/Débito**, **Dinheiro** ou **Boleto**.
- Atualização e renovação automática da data de validade do plano do atleta ao confirmar recebimento.
- Alertas de inadimplência e mensalidades a vencer.
- **Recibo Oficial Imprimível (`/Payments/Receipt/{id}`)**: Layout elegante com CNPJ da academia, dados do aluno, valor e espaço para assinatura.

### 4. 📅 Grade de Horários & Aulas de Boxe (`/Schedule`)
- Cronograma semanal de treinos (Segunda a Sábado).
- Segmentação por modalidade: *Fundamentos & Guarda*, *Sparring Técnico*, *Boxe Funcional / Aeróbico*, *Treino Livre de Ringue*.
- Controle de instrutor responsável, limite de atletas por turma e espaço (Ringue 1, Área de Sacos, etc.).

### 5. 📊 Dashboard Operacional da Recepção (`/`)
- Contadores de atletas ativos, alunos treinando agora, frequência do dia e faturamento do mês.
- Terminal de check-in rápido embutido diretamente na página inicial.
- Aulas programadas para o dia atual.

---

## 🛠️ Tecnologias Utilizadas
- **Linguagem**: C# (.NET 8.0 SDK)
- **Framework Web**: ASP.NET Core Razor Pages
- **ORM**: Entity Framework Core 8.0
- **Banco de Dados**: SQLite (`boxegym.db` - arquivo local leve e automático)
- **Interface**: Bootstrap 5 + Bootstrap Icons com Tema Dark Premium de Boxe (Preto, Carmesim e Dourado)
