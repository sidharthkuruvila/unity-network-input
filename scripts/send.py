import socket
import time
#UDP_IP = "127.0.0.1" 
UDP_IP = "192.168.0.104"
UDP_PORT = 9050
MESSAGE = "123"

print "UDP target IP:", UDP_IP
print "UDP target port:", UDP_PORT

sock = socket.socket(socket.AF_INET, # Internet
                        socket.SOCK_DGRAM) # UDP

for i in range(100):
    MESSAGE = ",".join(str(i) for i in [0, 1.55, i * 0.1])
    print MESSAGE
    sock.sendto(MESSAGE, (UDP_IP, UDP_PORT))
    time.sleep(0.01)
