import fs from 'node:fs';
import path from 'node:path';
import zlib from 'node:zlib';
import {Readable} from 'node:stream';
import {pipeline} from 'node:stream/promises';
// USTAR amb permisos d'execucio Unix, fins i tot quan s'empaqueta des de Windows.
const root=path.resolve(process.argv[2]), out=path.resolve(process.argv[3]);
function header(name,size,mode,dir){
 const b=Buffer.alloc(512);let prefix='';
 if(Buffer.byteLength(name)>100){const at=name.lastIndexOf('/');prefix=name.slice(0,at);name=name.slice(at+1);}
 if(Buffer.byteLength(name)>100||Buffer.byteLength(prefix)>155)throw Error('Ruta massa llarga per USTAR');
 const str=(v,o,n)=>b.write(v,o,n,'utf8');const oct=(v,o,n)=>str(v.toString(8).padStart(n-1,'0')+'\0',o,n);
 str(name,0,100);oct(mode,100,8);oct(0,108,8);oct(0,116,8);oct(size,124,12);oct(Math.floor(Date.now()/1000),136,12);
 b.fill(32,148,156);str(dir?'5':'0',156,1);str('ustar\0',257,6);str('00',263,2);str(prefix,345,155);
 const sum=b.reduce((a,v)=>a+v,0);str(sum.toString(8).padStart(6,'0')+'\0 ',148,8);return b;
}
async function* walk(dir,rel){
 yield header(rel+'/',0,0o755,true);
 for(const item of fs.readdirSync(dir,{withFileTypes:true})){
 const file=path.join(dir,item.name),name=rel+'/'+item.name;
 if(item.isDirectory())yield* walk(file,name);
 else {const size=fs.statSync(file).size;yield header(name,size,/\.(sh|x86_64)$/.test(item.name)?0o755:0o644,false);
 for await(const chunk of fs.createReadStream(file))yield chunk;
 if(size%512)yield Buffer.alloc(512-size%512);}
 }
}
async function* archive(){yield* walk(root,path.basename(root));yield Buffer.alloc(1024);}
await pipeline(Readable.from(archive()),zlib.createGzip({level:6}),fs.createWriteStream(out));
console.log(out);