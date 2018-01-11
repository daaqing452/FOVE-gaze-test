# -*- coding:utf-8 -*-
import socket
import threading

ip = socket.gethostbyname(socket.gethostname())
print('[   my ip    ]:', ip)

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect(('192.168.1.250', 10441))

def send(s):
	sock.sendall(bytes(s + '\n', encoding='utf-8'))

def recv(s):
	print('[   receive  ]: ' + s)

def recv_thread():
	print('[ connected  ]')
	while True:
		b = sock.recv(1024)
		if len(b) <= 0:
			print('[disconnected]')
			break
		s = str(b, encoding='utf-8')
		recv(s)

t = threading.Thread(target=recv_thread)
t.setDaemon(True)
t.start()

while True:
	s = input()
	print('[    send    ]: "%s"' % (s))
	send(s)