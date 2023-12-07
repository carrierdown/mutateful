var app=function(){"use strict";function t(){}function e(t){return t()}function n(){return Object.create(null)}function l(t){t.forEach(e)}function o(t){return"function"==typeof t}function r(t,e){return t!=t?e==e:t!==e||t&&"object"==typeof t||"function"==typeof t}function c(e,n,l){e.$$.on_destroy.push(function(e,...n){if(null==e)return t;const l=e.subscribe(...n);return l.unsubscribe?()=>l.unsubscribe():l}(n,l))}function i(t,e){t.appendChild(e)}function f(t,e,n){t.insertBefore(e,n||null)}function u(t){t.parentNode.removeChild(t)}function a(t,e){for(let n=0;n<t.length;n+=1)t[n]&&t[n].d(e)}function s(t){return document.createElement(t)}function p(t){return document.createTextNode(t)}function m(){return p(" ")}function h(t,e,n){null==n?t.removeAttribute(e):t.getAttribute(e)!==n&&t.setAttribute(e,n)}function d(t,e){e=""+e,t.wholeText!==e&&(t.data=e)}function g(t,e,n,l){t.style.setProperty(e,n,l?"important":"")}function $(t,e,n){t.classList[n?"add":"remove"](e)}let R;function b(t){R=t}function y(t){(function(){if(!R)throw new Error("Function called outside component initialization");return R})().$$.on_mount.push(t)}const v=[],w=[],x=[],_=[],k=Promise.resolve();let C=!1;function A(t){x.push(t)}let E=!1;const M=new Set;function B(){if(!E){E=!0;do{for(let t=0;t<v.length;t+=1){const e=v[t];b(e),I(e.$$)}for(b(null),v.length=0;w.length;)w.pop()();for(let t=0;t<x.length;t+=1){const e=x[t];M.has(e)||(M.add(e),e())}x.length=0}while(v.length);for(;_.length;)_.pop()();C=!1,E=!1,M.clear()}}function I(t){if(null!==t.fragment){t.update(),l(t.before_update);const e=t.dirty;t.dirty=[-1],t.fragment&&t.fragment.p(t.ctx,e),t.after_update.forEach(A)}}const H=new Set;let S;function U(){S={r:0,c:[],p:S}}function F(){S.r||l(S.c),S=S.p}function P(t,e){t&&t.i&&(H.delete(t),t.i(e))}function D(t,e,n,l){if(t&&t.o){if(H.has(t))return;H.add(t),S.c.push((()=>{H.delete(t),l&&(n&&t.d(1),l())})),t.o(e)}}function G(t){t&&t.c()}function J(t,n,r,c){const{fragment:i,on_mount:f,on_destroy:u,after_update:a}=t.$$;i&&i.m(n,r),c||A((()=>{const n=f.map(e).filter(o);u?u.push(...n):l(n),t.$$.on_mount=[]})),a.forEach(A)}function O(t,e){const n=t.$$;null!==n.fragment&&(l(n.on_destroy),n.fragment&&n.fragment.d(e),n.on_destroy=n.fragment=null,n.ctx=[])}function j(t,e){-1===t.$$.dirty[0]&&(v.push(t),C||(C=!0,k.then(B)),t.$$.dirty.fill(0)),t.$$.dirty[e/31|0]|=1<<e%31}function z(e,o,r,c,i,f,a,s=[-1]){const p=R;b(e);const m=e.$$={fragment:null,ctx:null,props:f,update:t,not_equal:i,bound:n(),on_mount:[],on_destroy:[],on_disconnect:[],before_update:[],after_update:[],context:new Map(p?p.$$.context:o.context||[]),callbacks:n(),dirty:s,skip_bound:!1,root:o.target||p.$$.root};a&&a(m.root);let h=!1;if(m.ctx=r?r(e,o.props||{},((t,n,...l)=>{const o=l.length?l[0]:n;return m.ctx&&i(m.ctx[t],m.ctx[t]=o)&&(!m.skip_bound&&m.bound[t]&&m.bound[t](o),h&&j(e,t)),n})):[],m.update(),h=!0,l(m.before_update),m.fragment=!!c&&c(m.ctx),o.target){if(o.hydrate){const t=function(t){return Array.from(t.childNodes)}(o.target);m.fragment&&m.fragment.l(t),t.forEach(u)}else m.fragment&&m.fragment.c();o.intro&&P(e.$$.fragment),J(e,o.target,o.anchor,o.customElement),B()}b(p)}class L{$destroy(){O(this,1),this.$destroy=t}$on(t,e){const n=this.$$.callbacks[t]||(this.$$.callbacks[t]=[]);return n.push(e),()=>{const t=n.indexOf(e);-1!==t&&n.splice(t,1)}}$set(t){var e;this.$$set&&(e=t,0!==Object.keys(e).length)&&(this.$$.skip_bound=!0,this.$$set(t),this.$$.skip_bound=!1)}}const N=[];const q=(new signalR.HubConnectionBuilder).withUrl("http://localhost:5000/mutatefulHub?username=webui").withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol).build();let T=null;q.on("SetClipDataOnWebUI",((t,e)=>{console.log("DebugMessage - received data: ",e,"clipRef",t),W(t,e)})),q.start().then((async()=>{console.log("I'm updated. Connection established, got",q.connectionId)}),(()=>{console.log("Failed to connect :(")}));const V=(()=>{const{subscribe:e,set:n}=function(e,n=t){let l;const o=new Set;function c(t){if(r(e,t)&&(e=t,l)){const t=!N.length;for(const t of o)t[1](),N.push(t,e);if(t){for(let t=0;t<N.length;t+=2)N[t][0](N[t+1]);N.length=0}}}return{set:c,update:function(t){c(t(e))},subscribe:function(r,i=t){const f=[r,i];return o.add(f),1===o.size&&(l=n(c)||t),r(e),()=>{o.delete(f),0===o.size&&(l(),l=null)}}}}({clipRef:"",data:new Uint8Array});return T=n,{subscribe:e,set:(t,e)=>{n({clipRef:t,data:e})}}})(),W=(t,e)=>{null==T||T({clipRef:t,data:e})};class K{constructor(t){this.notes=[],this.length=t}}class Q{constructor(t,e,n,l){this.pitch=t,this.start=e,this.duration=n,this.velocity=l}}function X(t){let e=Y(t,2),n=new K(e),l=function(t,e=0){let n=new Uint8Array(2);return n[0]=t[e],n[1]=t[e+1],new Uint16Array(n.buffer)[0]}(t,7),o=9;for(let e=0;e<l;e++)n.notes.push(new Q(t[o],Y(t,o+1),Y(t,o+5),Y(t,o+9))),o+=25;return n}function Y(t,e=0){let n=new Uint8Array(4);for(let l=0;l<4;l++)n[l]=t[l+e];return new Float32Array(n.buffer)[0]}function Z(t){let e,n=t[7](t[6].data)+"";return{c(){e=p(n)},m(t,n){f(t,e,n)},p(t,l){64&l&&n!==(n=t[7](t[6].data)+"")&&d(e,n)},d(t){t&&u(e)}}}function tt(e){let n,l,o,r,c,a,g,R,b,y=e[0].toUpperCase()+"",v=e[6].clipRef===e[0]&&Z(e);return{c(){n=m(),l=s("div"),o=s("div"),r=s("span"),c=p(y),a=s("span"),g=p(e[1]),R=m(),b=s("canvas"),v&&v.c(),h(r,"class","clip-slot--ref svelte-1hayvt1"),h(a,"class","clip-slot--title svelte-1hayvt1"),h(o,"class","clip-slot--header svelte-1hayvt1"),h(b,"class","clip-slot--preview svelte-1hayvt1"),h(b,"width",e[3]),h(b,"height",e[4]),$(b,"empty",e[5]),h(l,"class","clip-slot svelte-1hayvt1")},m(t,u){f(t,n,u),f(t,l,u),i(l,o),i(o,r),i(r,c),i(o,a),i(a,g),i(l,R),i(l,b),v&&v.m(b,null),e[8](b)},p(t,[e]){1&e&&y!==(y=t[0].toUpperCase()+"")&&d(c,y),2&e&&d(g,t[1]),t[6].clipRef===t[0]?v?v.p(t,e):(v=Z(t),v.c(),v.m(b,null)):v&&(v.d(1),v=null),8&e&&h(b,"width",t[3]),16&e&&h(b,"height",t[4]),32&e&&$(b,"empty",t[5])},i:t,o:t,d(t){t&&u(n),t&&u(l),v&&v.d(),e[8](null)}}}function et(t,e,n){let l;c(t,V,(t=>n(6,l=t)));let o,{clipRef:r}=e,{formula:i=""}=e,f=300,u=150,a=!0;y((()=>{let t=getComputedStyle(o);n(3,f=parseInt(t.getPropertyValue("width"),10)),n(4,u=parseInt(t.getPropertyValue("height"),10))}));return t.$$set=t=>{"clipRef"in t&&n(0,r=t.clipRef),"formula"in t&&n(1,i=t.formula)},[r,i,o,f,u,a,l,t=>((t=>{n(5,a=!1);const e=o.getContext("2d");e.clearRect(0,0,f,u),e.fillStyle="#B4B1B0";let l=Math.min(t.length,16),r=t.notes.map((t=>t.pitch)),c=Math.max(...r),i=Math.min(...r),s=Math.max(c-i,5),p=f/l,m=u/(s+1);for(let n of t.notes){if(n.start>=l)return void console.log("skipping");e.fillRect(Math.floor(p*n.start),Math.floor(m*(s-(n.pitch-i))),Math.floor(p*n.duration),Math.floor(m))}})(X(t)),""),function(t){w[t?"unshift":"push"]((()=>{o=t,n(2,o)}))}]}class nt extends L{constructor(t){super(),z(this,t,et,tt,r,{clipRef:0,formula:1})}}function lt(t,e,n){const l=t.slice();return l[1]=e[n],l}function ot(t,e,n){const l=t.slice();return l[4]=e[n],l}function rt(e){let n,l,o;return l=new nt({props:{clipRef:e[4].clipRef,formula:e[4].formula}}),{c(){n=s("td"),G(l.$$.fragment),h(n,"class","svelte-8k2hbi")},m(t,e){f(t,n,e),J(l,n,null),o=!0},p:t,i(t){o||(P(l.$$.fragment,t),o=!0)},o(t){D(l.$$.fragment,t),o=!1},d(t){t&&u(n),O(l)}}}function ct(t){let e,n,l,o=t[1],r=[];for(let e=0;e<o.length;e+=1)r[e]=rt(ot(t,o,e));const c=t=>D(r[t],1,1,(()=>{r[t]=null}));return{c(){e=s("tr");for(let t=0;t<r.length;t+=1)r[t].c();n=m()},m(t,o){f(t,e,o);for(let t=0;t<r.length;t+=1)r[t].m(e,null);i(e,n),l=!0},p(t,l){if(1&l){let i;for(o=t[1],i=0;i<o.length;i+=1){const c=ot(t,o,i);r[i]?(r[i].p(c,l),P(r[i],1)):(r[i]=rt(c),r[i].c(),P(r[i],1),r[i].m(e,n))}for(U(),i=o.length;i<r.length;i+=1)c(i);F()}},i(t){if(!l){for(let t=0;t<o.length;t+=1)P(r[t]);l=!0}},o(t){r=r.filter(Boolean);for(let t=0;t<r.length;t+=1)D(r[t]);l=!1},d(t){t&&u(e),a(r,t)}}}function it(t){let e,n,l,o=t[0],r=[];for(let e=0;e<o.length;e+=1)r[e]=ct(lt(t,o,e));const c=t=>D(r[t],1,1,(()=>{r[t]=null}));return{c(){e=m(),n=s("table");for(let t=0;t<r.length;t+=1)r[t].c();h(n,"class","main-cell-table svelte-8k2hbi")},m(t,o){f(t,e,o),f(t,n,o);for(let t=0;t<r.length;t+=1)r[t].m(n,null);l=!0},p(t,[e]){if(1&e){let l;for(o=t[0],l=0;l<o.length;l+=1){const c=lt(t,o,l);r[l]?(r[l].p(c,e),P(r[l],1)):(r[l]=ct(c),r[l].c(),P(r[l],1),r[l].m(n,null))}for(U(),l=o.length;l<r.length;l+=1)c(l);F()}},i(t){if(!l){for(let t=0;t<o.length;t+=1)P(r[t]);l=!0}},o(t){r=r.filter(Boolean);for(let t=0;t<r.length;t+=1)D(r[t]);l=!1},d(t){t&&u(e),t&&u(n),a(r,t)}}}function ft(t){return[[[{formula:"",clipRef:"A1"},{formula:"",clipRef:"B1"},{formula:"",clipRef:"C1"},{formula:"",clipRef:"D1"},{formula:"",clipRef:"E1"},{formula:"",clipRef:"F1"},{formula:"",clipRef:"G1"},{formula:"",clipRef:"H1"},{formula:"",clipRef:"I1"},{formula:"",clipRef:"J1"}],[{formula:"",clipRef:"A2"},{formula:"",clipRef:"B2"},{formula:"",clipRef:"C2"},{formula:"",clipRef:"D2"},{formula:"",clipRef:"E2"},{formula:"",clipRef:"F1"},{formula:"",clipRef:"G1"},{formula:"",clipRef:"H1"},{formula:"",clipRef:"I1"},{formula:"",clipRef:"J1"}],[{formula:"",clipRef:"A3"},{formula:"",clipRef:"B3"},{formula:"",clipRef:"C3"},{formula:"",clipRef:"D3"},{formula:"",clipRef:"E3"},{formula:"",clipRef:"F1"},{formula:"",clipRef:"G1"},{formula:"",clipRef:"H1"},{formula:"",clipRef:"I1"},{formula:"",clipRef:"J1"}],[{formula:"",clipRef:"A4"},{formula:"",clipRef:"B4"},{formula:"",clipRef:"C4"},{formula:"",clipRef:"D4"},{formula:"",clipRef:"E4"},{formula:"",clipRef:"F1"},{formula:"",clipRef:"G1"},{formula:"",clipRef:"H1"},{formula:"",clipRef:"I1"},{formula:"",clipRef:"J1"}]]]}class ut extends L{constructor(t){super(),z(this,t,ft,it,r,{})}}function at(e){let n,l,o,r,c,i,a;return{c(){n=s("code"),n.textContent="M",l=m(),o=s("input"),r=m(),c=s("div"),h(n,"class","char-width-reference svelte-170irtv"),h(o,"class","formula-editor svelte-170irtv"),h(o,"type","text"),h(c,"class","autocomplete-list svelte-170irtv"),g(c,"left",e[0]+"px"),g(c,"display","none")},m(t,u){var s,p,m,h;f(t,n,u),f(t,l,u),f(t,o,u),f(t,r,u),f(t,c,u),i||(s=o,p="keyup",m=e[1],s.addEventListener(p,m,h),a=()=>s.removeEventListener(p,m,h),i=!0)},p(t,[e]){1&e&&g(c,"left",t[0]+"px")},i:t,o:t,d(t){t&&u(n),t&&u(l),t&&u(o),t&&u(r),t&&u(c),i=!1,a()}}}function st(t,e,n){let l=0,o=8;return y((()=>{o=document.querySelector(".char-width-reference").offsetWidth})),[l,function(t){n(0,l=o*t.target.selectionStart),l>0&&n(0,l-=4)}]}class pt extends L{constructor(t){super(),z(this,t,st,at,r,{})}}function mt(e){let n,l,o,r,c;return l=new pt({}),r=new ut({}),{c(){n=s("div"),G(l.$$.fragment),o=m(),G(r.$$.fragment),h(n,"class","container svelte-1y12bcm")},m(t,e){f(t,n,e),J(l,n,null),i(n,o),J(r,n,null),c=!0},p:t,i(t){c||(P(l.$$.fragment,t),P(r.$$.fragment,t),c=!0)},o(t){D(l.$$.fragment,t),D(r.$$.fragment,t),c=!1},d(t){t&&u(n),O(l),O(r)}}}return new class extends L{constructor(t){super(),z(this,t,null,mt,r,{})}}({target:document.body,props:{name:"world"}})}();
//# sourceMappingURL=index.js.map
