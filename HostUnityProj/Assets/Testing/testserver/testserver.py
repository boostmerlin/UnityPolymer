import socketserver
import time

class TcpHandler(socketserver.BaseRequestHandler):
    def setup(self):
        print("{}:{} connected..".format(self.client_address[0], self.client_address[1]))
    def finish(self):
        print("client may disconnected")

    def handle(self):
        print("in handle")
        while True:
            print("about to recv data...")
            data = self.request.recv(2048)
            if data:
                print(data)
                self.request.sendall(data.upper())
            else:
                break
            time.sleep(0.1)

if __name__ == "__main__":
    HOST, PORT = "localhost", 9999
    with socketserver.TCPServer((HOST, PORT), TcpHandler) as server:
        print("server listen on: ", HOST, PORT)
        server.serve_forever()
