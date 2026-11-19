# Alternativa a package-linux.mjs quan no hi ha Node: TAR.GZ amb permisos Unix
# (755 per a carpetes, .sh i .x86_64; 644 per a la resta), encara que s'empaquete des de Windows.
import os, sys, tarfile, time

root, out = os.path.abspath(sys.argv[1]), os.path.abspath(sys.argv[2])
base = os.path.basename(root)
now = int(time.time())

def info(path, name, is_dir):
    t = tarfile.TarInfo(name)
    t.mtime = now
    t.uid = t.gid = 0
    t.uname = t.gname = ""
    if is_dir:
        t.type, t.mode = tarfile.DIRTYPE, 0o755
    else:
        t.size = os.path.getsize(path)
        t.mode = 0o755 if name.endswith((".sh", ".x86_64")) else 0o644
    return t

with tarfile.open(out, "w:gz", format=tarfile.USTAR_FORMAT, compresslevel=6) as tar:
    for dirpath, dirnames, filenames in os.walk(root):
        dirnames.sort()
        rel = os.path.relpath(dirpath, root)
        arc = base if rel == "." else base + "/" + rel.replace(os.sep, "/")
        tar.addfile(info(dirpath, arc, True))
        for f in sorted(filenames):
            p = os.path.join(dirpath, f)
            with open(p, "rb") as fh:
                tar.addfile(info(p, arc + "/" + f, False), fh)
print(out)
