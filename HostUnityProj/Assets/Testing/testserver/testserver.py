# -*- coding:utf-8 -*-
import socketserver
import time
import HelloRpc_pb2
import MyPolymerizedUniframeMsgDefine as MsgDefine
import google.protobuf
import sys
import socket

headLen = 8

def decodeHead(data):
    #len
    length = int.from_bytes(data[0:4], "big")
    print("msg len = ", length)
    msgid = int.from_bytes(data[4:headLen], "big")
    print("recv msgid=", msgid, "HelloRequest = 1033795188")
    return msgid

def decodeMsg(data):
    req = HelloRpc_pb2.HelloRequest()
    req.ParseFromString(data[headLen:])
    return req
    
def sendResponse(request):
    res = HelloRpc_pb2.HelloResponse()
    res.number[:] = (1, 0, 2, 4)
    res.reply = "hello client"
    data = bytearray(res.ByteSize() + headLen)
    data[0:4] = int.to_bytes(headLen+res.ByteSize(), 4, "big")
    data[4:headLen] = int.to_bytes(MsgDefine.HelloResponse, 4, "big")
    data[headLen:] = res.SerializeToString()
    print("send data len=", len(data))
    request.sendall(data)
  
class TcpHandler(socketserver.BaseRequestHandler):
    def setup(self):
        print("{}:{} connected..".format(self.client_address[0], self.client_address[1]))
    def finish(self):
        print("client may disconnected")

    def handle(self):
        print("in handle")
        while True:
            print("about to recv data...")
            data = self.request.recv(4096)
            if data:
                dataarray = bytearray(data)
                msgid = decodeHead(dataarray)
                print("recv msgid:", msgid)
                msg = decodeMsg(dataarray)
                print(str(msg))
                sendResponse(self.request)
            else:
                break
            time.sleep(0.1)

if __name__ == "__main__":
    #log proto info
    HOST, PORT = "localhost", 9999
    print("my machine byte order: ", sys.byteorder)
    with socketserver.TCPServer((HOST, PORT), TcpHandler) as server:
        print("server listen on: ", HOST, PORT)
        server.serve_forever()
