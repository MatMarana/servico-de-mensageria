# Padrões do projeto
#### Nomes de Branch
Cada linguagem deve ter sua branch main e suas branchs para cada etapa do projeto
**Branch main:** Deve seguir o formato `{linguagem}/main`.
**Exemplo:** `C#/main`

**Branch de desenvolvimento**: Deve seguir o formato `{linguagem}/etapa`
**Exemplo:** `C#/request-reply`

***
#### Mensagens do cliente
Os clientes devem sempre mandar as mensagens nos mesmos padrões, pois o cliente de um pode mandar mensagem para o servidor de outro.
Para melhorar o debug e identificar qual cliente está mandando a mensagem adicionamos a linguagem que está enviando na hora de printar a mensagem no terminal.

**Padrão de envio:** `{Operação}|{conteudo}|{timestamp}`
**Exemplo:** `login|albertini|19:17:21`

**Padrão de print no terminal:** `{linguagem}: {mensagem}`
**Exemplo:** `C-Sharp: login|albertini|19:17:21`

**Todas as mensagens devem ser enviadas em letras minusculas**.

***
#### Mensagens do servidor
Os servidores devem sempre mandar as respostas nos mesmos padrões, pois o servidor feito em uma linguagem pode responder o cliente feito em outra.
Para melhorar o debug e identificar qual servidor está respondendo a mensagem adicionamos a linguagem que está enviando na hora de printar a mensagem no terminal.

**Padrão de envio:**
A resposta do servidor pode variar de acordo com a operacao enviada pelo cliente, as possíveis respostas são:
**Caso operação = login:**
- "login" se o conteúdo ainda não existir no servidor
- "erro" se já existir

**Caso operação = canais:**
- "sucesso" se o conteúdo ainda não existir no servidor
- "erro" se já existir

**Caso operação = listar:**
- [lista de todos os canais do servidor]


**Padrão de print no terminal:** `{linguagem}: {resposta}`
**Exemplo:** `C-Sharp: erro`

**Todas as mensagens devem ser enviadas em letras minusculas**.