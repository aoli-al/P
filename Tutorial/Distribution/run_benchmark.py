import subprocess
import os
from multiprocessing import Pool
import shutil
import sys
import glob

HOME = os.path.expanduser('~')

def run_test():
    os.mkdir("logs")
    for i in range(10000):
        if i % 100 == 0:
            print(i)
        out = open(os.path.join("logs", f"{i}.txt"), "w")
        command = ["dotnet", f"{HOME}/repos/P/Bld/Drops/Release/Binaries/net6.0/p.dll", "check", "--sch-pattern", "-v", "-tc", "tc"]
        subprocess.call(command, stdout=out)


if __name__ == "__main__":
    shutil.rmtree("logs", ignore_errors=True)
    run_test()




