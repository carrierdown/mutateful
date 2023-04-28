var app=function(){"use strict";function t(){}function e(t){return t()}function n(){return Object.create(null)}function l(t){t.forEach(e)}function o(t){return"function"==typeof t}function r(t,e){return t!=t?e==e:t!==e||t&&"object"==typeof t||"function"==typeof t}function c(e,n,l){e.$$.on_destroy.push(function(e,...n){if(null==e)return t;const l=e.subscribe(...n);return l.unsubscribe?()=>l.unsubscribe():l}(n,l))}function i(t,e){t.appendChild(e)}function f(t,e,n){t.insertBefore(e,n||null)}function u(t){t.parentNode.removeChild(t)}function a(t,e){for(let n=0;n<t.length;n+=1)t[n]&&t[n].d(e)}function s(t){return document.createElement(t)}function p(t){return document.createTextNode(t)}function d(){return p(" ")}function m(t,e,n){null==n?t.removeAttribute(e):t.getAttribute(e)!==n&&t.setAttribute(e,n)}function h(t,e){e=""+e,t.wholeText!==e&&(t.data=e)}function g(t,e,n,l){t.style.setProperty(e,n,l?"important":"")}function $(t,e,n){t.classList[n?"add":"remove"](e)}let R;function b(t){R=t}function y(){if(!R)throw new Error("Function called outside component initialization");return R}function v(t){y().$$.on_mount.push(t)}const w=[],x=[],_=[],C=[],A=Promise.resolve();let E=!1;function k(t){_.push(t)}let M=!1;const H=new Set;function S(){if(!M){M=!0;do{for(let t=0;t<w.length;t+=1){const e=w[t];b(e),U(e.$$)}for(b(null),w.length=0;x.length;)x.pop()();for(let t=0;t<_.length;t+=1){const e=_[t];H.has(e)||(H.add(e),e())}_.length=0}while(w.length);for(;C.length;)C.pop()();E=!1,M=!1,H.clear()}}function U(t){if(null!==t.fragment){t.update(),l(t.before_update);const e=t.dirty;t.dirty=[-1],t.fragment&&t.fragment.p(t.ctx,e),t.after_update.forEach(k)}}const B=new Set;let I;function F(){I={r:0,c:[],p:I}}function P(){I.r||l(I.c),I=I.p}function D(t,e){t&&t.i&&(B.delete(t),t.i(e))}function j(t,e,n,l){if(t&&t.o){if(B.has(t))return;B.add(t),I.c.push((()=>{B.delete(t),l&&(n&&t.d(1),l())})),t.o(e)}}function G(t){t&&t.c()}function J(t,n,r,c){const{fragment:i,on_mount:f,on_destroy:u,after_update:a}=t.$$;i&&i.m(n,r),c||k((()=>{const n=f.map(e).filter(o);u?u.push(...n):l(n),t.$$.on_mount=[]})),a.forEach(k)}function O(t,e){const n=t.$$;null!==n.fragment&&(l(n.on_destroy),n.fragment&&n.fragment.d(e),n.on_destroy=n.fragment=null,n.ctx=[])}function z(t,e){-1===t.$$.dirty[0]&&(w.push(t),E||(E=!0,A.then(S)),t.$$.dirty.fill(0)),t.$$.dirty[e/31|0]|=1<<e%31}function L(e,o,r,c,i,f,a,s=[-1]){const p=R;b(e);const d=e.$$={fragment:null,ctx:null,props:f,update:t,not_equal:i,bound:n(),on_mount:[],on_destroy:[],on_disconnect:[],before_update:[],after_update:[],context:new Map(p?p.$$.context:o.context||[]),callbacks:n(),dirty:s,skip_bound:!1,root:o.target||p.$$.root};a&&a(d.root);let m=!1;if(d.ctx=r?r(e,o.props||{},((t,n,...l)=>{const o=l.length?l[0]:n;return d.ctx&&i(d.ctx[t],d.ctx[t]=o)&&(!d.skip_bound&&d.bound[t]&&d.bound[t](o),m&&z(e,t)),n})):[],d.update(),m=!0,l(d.before_update),d.fragment=!!c&&c(d.ctx),o.target){if(o.hydrate){const t=function(t){return Array.from(t.childNodes)}(o.target);d.fragment&&d.fragment.l(t),t.forEach(u)}else d.fragment&&d.fragment.c();o.intro&&D(e.$$.fragment),J(e,o.target,o.anchor,o.customElement),S()}b(p)}class N{$destroy(){O(this,1),this.$destroy=t}$on(t,e){const n=this.$$.callbacks[t]||(this.$$.callbacks[t]=[]);return n.push(e),()=>{const t=n.indexOf(e);-1!==t&&n.splice(t,1)}}$set(t){var e;this.$$set&&(e=t,0!==Object.keys(e).length)&&(this.$$.skip_bound=!0,this.$$set(t),this.$$.skip_bound=!1)}}const q=[];const T=(new signalR.HubConnectionBuilder).withUrl("http://localhost:5000/mutatefulHub").withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol).build();let V=null;T.on("SetClipDataOnWebUI",((t,e)=>{console.log("DebugMessage - received data: ",e,"clipRef",t),K(t,e)})),T.start().then((async()=>{console.log("Connection established, got",T.connectionId)}),(()=>{console.log("Failed to connect :(")}));const W=(()=>{const{subscribe:e,set:n}=function(e,n=t){let l;const o=new Set;function c(t){if(r(e,t)&&(e=t,l)){const t=!q.length;for(const t of o)t[1](),q.push(t,e);if(t){for(let t=0;t<q.length;t+=2)q[t][0](q[t+1]);q.length=0}}}return{set:c,update:function(t){c(t(e))},subscribe:function(r,i=t){const f=[r,i];return o.add(f),1===o.size&&(l=n(c)||t),r(e),()=>{o.delete(f),0===o.size&&(l(),l=null)}}}}({clipRef:"",data:new Uint8Array});return V=n,{subscribe:e,set:(t,e)=>{n({clipRef:t,data:e})}}})(),K=(t,e)=>{null==V||V({clipRef:t,data:e})};class Q{constructor(t){this.notes=[],this.length=t}}class X{constructor(t,e,n,l){this.pitch=t,this.start=e,this.duration=n,this.velocity=l}}function Y(t){let e=Z(t,2),n=new Q(e),l=function(t,e=0){let n=new Uint8Array(2);return n[0]=t[e],n[1]=t[e+1],new Uint16Array(n.buffer)[0]}(t,7),o=9;for(let e=0;e<l;e++)n.notes.push(new X(t[o],Z(t,o+1),Z(t,o+5),Z(t,o+9))),o+=25;return n}function Z(t,e=0){let n=new Uint8Array(4);for(let l=0;l<4;l++)n[l]=t[l+e];return new Float32Array(n.buffer)[0]}function tt(t){let e,n=t[7](t[6].data)+"";return{c(){e=p(n)},m(t,n){f(t,e,n)},p(t,l){64&l&&n!==(n=t[7](t[6].data)+"")&&h(e,n)},d(t){t&&u(e)}}}function et(e){let n,l,o,r,c,a,g,R,b,y=e[0].toUpperCase()+"",v=e[6].clipRef===e[0]&&tt(e);return{c(){n=d(),l=s("div"),o=s("div"),r=s("span"),c=p(y),a=s("span"),g=p(e[1]),R=d(),b=s("canvas"),v&&v.c(),m(r,"class","clip-slot--ref svelte-a8blad"),m(a,"class","clip-slot--title svelte-a8blad"),m(o,"class","clip-slot--header svelte-a8blad"),m(b,"class","clip-slot--preview svelte-a8blad"),m(b,"width",e[3]),m(b,"height",e[4]),$(b,"empty",e[5]),m(l,"class","clip-slot svelte-a8blad")},m(t,u){f(t,n,u),f(t,l,u),i(l,o),i(o,r),i(r,c),i(o,a),i(a,g),i(l,R),i(l,b),v&&v.m(b,null),e[8](b)},p(t,[e]){1&e&&y!==(y=t[0].toUpperCase()+"")&&h(c,y),2&e&&h(g,t[1]),t[6].clipRef===t[0]?v?v.p(t,e):(v=tt(t),v.c(),v.m(b,null)):v&&(v.d(1),v=null),8&e&&m(b,"width",t[3]),16&e&&m(b,"height",t[4]),32&e&&$(b,"empty",t[5])},i:t,o:t,d(t){t&&u(n),t&&u(l),v&&v.d(),e[8](null)}}}function nt(t,e,n){let l;c(t,W,(t=>n(6,l=t)));let{clipRef:o}=e,{formula:r=""}=e;const i=W.subscribe((({ref:t,data:e})=>{console.log("Hello"),o===t&&console.log("Hello we have a match")}));let f,u=!1,a=300,s=150,p=!0;var d;v((()=>{u=!0;let t=getComputedStyle(f);n(3,a=parseInt(t.getPropertyValue("width"),10)),n(4,s=parseInt(t.getPropertyValue("height"),10))})),d=i,y().$$.on_destroy.push(d);return t.$$set=t=>{"clipRef"in t&&n(0,o=t.clipRef),"formula"in t&&n(1,r=t.formula)},[o,r,f,a,s,p,l,t=>(u&&(console.log("Updating clip"),(t=>{n(5,p=!1);const e=f.getContext("2d");console.log(a,s),e.clearRect(0,0,a,s),e.fillStyle="white",e.fillRect(0,0,a,s),e.fillStyle="red";let l=Math.min(t.length,16),o=t.notes.map((t=>t.pitch)),r=Math.max(...o),c=Math.min(...o),i=a/l,u=s/(r+1-c);for(let n of t.notes){if(n.start>=l)return;e.fillRect(Math.floor(i*n.start),Math.floor(u*(n.pitch-c)),Math.floor(i),Math.floor(u))}})(Y(t))),""),function(t){x[t?"unshift":"push"]((()=>{f=t,n(2,f)}))}]}class lt extends N{constructor(t){super(),L(this,t,nt,et,r,{clipRef:0,formula:1})}}function ot(t,e,n){const l=t.slice();return l[1]=e[n],l}function rt(t,e,n){const l=t.slice();return l[4]=e[n],l}function ct(e){let n,l,o;return l=new lt({props:{clipRef:e[4].clipRef,formula:e[4].formula}}),{c(){n=s("td"),G(l.$$.fragment),m(n,"class","svelte-idc0c8")},m(t,e){f(t,n,e),J(l,n,null),o=!0},p:t,i(t){o||(D(l.$$.fragment,t),o=!0)},o(t){j(l.$$.fragment,t),o=!1},d(t){t&&u(n),O(l)}}}function it(t){let e,n,l,o=t[1],r=[];for(let e=0;e<o.length;e+=1)r[e]=ct(rt(t,o,e));const c=t=>j(r[t],1,1,(()=>{r[t]=null}));return{c(){e=s("tr");for(let t=0;t<r.length;t+=1)r[t].c();n=d()},m(t,o){f(t,e,o);for(let t=0;t<r.length;t+=1)r[t].m(e,null);i(e,n),l=!0},p(t,l){if(1&l){let i;for(o=t[1],i=0;i<o.length;i+=1){const c=rt(t,o,i);r[i]?(r[i].p(c,l),D(r[i],1)):(r[i]=ct(c),r[i].c(),D(r[i],1),r[i].m(e,n))}for(F(),i=o.length;i<r.length;i+=1)c(i);P()}},i(t){if(!l){for(let t=0;t<o.length;t+=1)D(r[t]);l=!0}},o(t){r=r.filter(Boolean);for(let t=0;t<r.length;t+=1)j(r[t]);l=!1},d(t){t&&u(e),a(r,t)}}}function ft(t){let e,n,l,o=t[0],r=[];for(let e=0;e<o.length;e+=1)r[e]=it(ot(t,o,e));const c=t=>j(r[t],1,1,(()=>{r[t]=null}));return{c(){e=d(),n=s("table");for(let t=0;t<r.length;t+=1)r[t].c();m(n,"class","main-cell-table svelte-idc0c8")},m(t,o){f(t,e,o),f(t,n,o);for(let t=0;t<r.length;t+=1)r[t].m(n,null);l=!0},p(t,[e]){if(1&e){let l;for(o=t[0],l=0;l<o.length;l+=1){const c=ot(t,o,l);r[l]?(r[l].p(c,e),D(r[l],1)):(r[l]=it(c),r[l].c(),D(r[l],1),r[l].m(n,null))}for(F(),l=o.length;l<r.length;l+=1)c(l);P()}},i(t){if(!l){for(let t=0;t<o.length;t+=1)D(r[t]);l=!0}},o(t){r=r.filter(Boolean);for(let t=0;t<r.length;t+=1)j(r[t]);l=!1},d(t){t&&u(e),t&&u(n),a(r,t)}}}function ut(t){return[[[{formula:"",clipRef:"A1"},{formula:"",clipRef:"B1"},{formula:"",clipRef:"C1"},{formula:"",clipRef:"D1"},{formula:"",clipRef:"E1"},{formula:"",clipRef:"F1"},{formula:"",clipRef:"G1"},{formula:"",clipRef:"H1"},{formula:"",clipRef:"I1"},{formula:"",clipRef:"J1"}],[{formula:"",clipRef:"A2"},{formula:"",clipRef:"B2"},{formula:"",clipRef:"C2"},{formula:"",clipRef:"D2"},{formula:"",clipRef:"E2"},{formula:"",clipRef:"F1"},{formula:"",clipRef:"G1"},{formula:"",clipRef:"H1"},{formula:"",clipRef:"I1"},{formula:"",clipRef:"J1"}],[{formula:"",clipRef:"A3"},{formula:"",clipRef:"B3"},{formula:"",clipRef:"C3"},{formula:"",clipRef:"D3"},{formula:"",clipRef:"E3"},{formula:"",clipRef:"F1"},{formula:"",clipRef:"G1"},{formula:"",clipRef:"H1"},{formula:"",clipRef:"I1"},{formula:"",clipRef:"J1"}],[{formula:"",clipRef:"A4"},{formula:"",clipRef:"B4"},{formula:"",clipRef:"C4"},{formula:"",clipRef:"D4"},{formula:"",clipRef:"E4"},{formula:"",clipRef:"F1"},{formula:"",clipRef:"G1"},{formula:"",clipRef:"H1"},{formula:"",clipRef:"I1"},{formula:"",clipRef:"J1"}]]]}class at extends N{constructor(t){super(),L(this,t,ut,ft,r,{})}}function st(e){let n,l,o,r,c,i,a;return{c(){n=s("code"),n.textContent="M",l=d(),o=s("input"),r=d(),c=s("div"),m(n,"class","char-width-reference svelte-170irtv"),m(o,"class","formula-editor svelte-170irtv"),m(o,"type","text"),m(c,"class","autocomplete-list svelte-170irtv"),g(c,"left",e[0]+"px"),g(c,"display","none")},m(t,u){var s,p,d,m;f(t,n,u),f(t,l,u),f(t,o,u),f(t,r,u),f(t,c,u),i||(s=o,p="keyup",d=e[1],s.addEventListener(p,d,m),a=()=>s.removeEventListener(p,d,m),i=!0)},p(t,[e]){1&e&&g(c,"left",t[0]+"px")},i:t,o:t,d(t){t&&u(n),t&&u(l),t&&u(o),t&&u(r),t&&u(c),i=!1,a()}}}function pt(t,e,n){let l=0,o=8;return v((()=>{o=document.querySelector(".char-width-reference").offsetWidth})),[l,function(t){n(0,l=o*t.target.selectionStart),l>0&&n(0,l-=4)}]}class dt extends N{constructor(t){super(),L(this,t,pt,st,r,{})}}function mt(e){let n,l,o,r,c;return l=new dt({}),r=new at({}),{c(){n=s("div"),G(l.$$.fragment),o=d(),G(r.$$.fragment),m(n,"class","container svelte-nrcuj4")},m(t,e){f(t,n,e),J(l,n,null),i(n,o),J(r,n,null),c=!0},p:t,i(t){c||(D(l.$$.fragment,t),D(r.$$.fragment,t),c=!0)},o(t){j(l.$$.fragment,t),j(r.$$.fragment,t),c=!1},d(t){t&&u(n),O(l),O(r)}}}return new class extends N{constructor(t){super(),L(this,t,null,mt,r,{})}}({target:document.body,props:{name:"world"}})}();
//# sourceMappingURL=index.js.map
