module Utils

  def self.cria_request(operacao, conteudo)
    time = Time.now.strftime("%d-%m-%Y %H:%M:%S")
    mensagem = "#{operacao} | #{conteudo} | #{time}"
   
    request = MensagemProto::EnviaMensagem.new("mensagem": mensagem)
    request_binario = MensagemProto::EnviaMensagem.encode(request)
   
    return request_binario
  end

  def self.recebe_request(socket)
    parts = []
    
    socket.recv_strings(parts)
    mensagem = MensagemProto::EnviaMensagem.decode(parts.last)
    
    return mensagem
  end

end
