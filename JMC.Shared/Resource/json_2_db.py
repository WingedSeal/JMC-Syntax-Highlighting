import pandas as pd
from glob import glob
import os
import pathlib
import sqlite3

# set path to current directory
abspath = os.path.abspath(__file__)
dname = os.path.dirname(abspath)
os.chdir(dname)

# database connection
db_name = "jmc.db"
try:
    os.remove(db_name)
except OSError:
    pass
con = sqlite3.Connection(db_name)

# minecraft
filePaths = glob("minecraft\\**\\*.json", recursive=True)
versions = []
mcDatas = {}
for i, p in enumerate(filePaths):
    with open(p, "r", encoding="utf-8") as f:
        s = f.read()
        path = pathlib.Path(p)
        version = path.parts[1]
        if version not in versions:
            versions.append(version)
        paths = "\\".join(path.parts[2:])[:-5]
        mcDatas.setdefault(version, {"path": [], "text": []})
        mcDatas[version]["path"].append(paths)
        mcDatas[version]["text"].append(s)

for i, v in enumerate(versions):
    df = pd.DataFrame.from_dict(mcDatas[v])
    df.to_sql("v" + v, con, index=False)

# jmc
path = "jmc/BuiltInFunctions.json"
with open(path, "r", encoding="utf-8") as f:
    s = f.read()
    p = "BuiltInFunctions"
    d = {"path": [p], "text": [s]}
    df = pd.DataFrame.from_dict(d)
    df.to_sql("jmc", con, index=False)
