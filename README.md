# Etapas de desenvolvimento

## Request-Reply
## Pub-Sub
Para essa etapa devemos implementar o pub-sub adicionando um proxy onde cada cliente irá se inscrever (sub) e cada servidor irá se associar (pub).

## Alterações necessárias:
- **Padronizar o processo de login:** Precisamos criar uma lógica única para o envio do login pelo cliente e criar uma lógica única para a validação do login pelo servidor.
- **Alterar o processo de criação de canais:** Cada cliente deverá ter uma lista/arquivo.txt que contenha 7 canais, todos os canais devem ser enviados para os servidores, sem validação necessária, apenas envia e o servidor apenas guarda. Para evitar problemas vamos garantir que não tenha canais repetidos entre as listas/arquivos.txt
- **Listagem de canais apenas uma vez:** Cada cliente deverá pedir a listagem de canais apenas uma vez, armazenar essa lista e escolher, aleatoriamente, 3 desses canais para se inscrever.

# Novas funcionalidades
- **Implementar o proxy:** Vamos montar um novo container para o proxy, podemos fazer ele em python mesmo para facilitar, como iremos utilziar containers separados as portas podem ser as mesmas (5555 e 5556)
- **Se inscrever em canais:** Os clientes devem se inscrever nos canais que querem receber as mensagens.
- **Envio de mensagens dos canais:** Os clientes para devem constantemente enviando requisições de mensagem que eles querem que o servidor publique, esse envio de requisição deverá ser através do brocker
- **Publicação de mensagens:** Os servidores irão receber a requisição e publicar no canal (proxy) e depois enviar uma resposta para o cliente através do brocker.
- **Receber mensagens:** Como os clientes irão se inscrever em canais eles vão receber as mensagens através do proxy (inclusive mensagens que ele mesmo fez a requisição de existir)
- **Persitência de dados:** Os servidores devem armazenar as mensagens enviadas em um txt, cada servidor deve ter seus próprios txts.

