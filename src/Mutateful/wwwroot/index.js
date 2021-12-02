var app=function(){"use strict";function t(){}function e(t){return t()}function n(){return Object.create(null)}function o(t){t.forEach(e)}function r(t){return"function"==typeof t}function l(t,e){return t!=t?e==e:t!==e||t&&"object"==typeof t||"function"==typeof t}function c(e,n,o){e.$$.on_destroy.push(function(e,...n){if(null==e)return t;const o=e.subscribe(...n);return o.unsubscribe?()=>o.unsubscribe():o}(n,o))}function s(t,e){t.appendChild(e)}function i(t,e,n){t.insertBefore(e,n||null)}function u(t){t.parentNode.removeChild(t)}function f(t){return document.createElement(t)}function a(t){return document.createTextNode(t)}function p(){return a(" ")}function $(t,e,n){null==n?t.removeAttribute(e):t.getAttribute(e)!==n&&t.setAttribute(e,n)}function d(t,e){e=""+e,t.wholeText!==e&&(t.data=e)}let h;function g(t){h=t}function m(t){(function(){if(!h)throw new Error("Function called outside component initialization");return h})().$$.on_mount.push(t)}const b=[],w=[],y=[],_=[],x=Promise.resolve();let A=!1;function R(t){y.push(t)}let v=!1;const C=new Set;function k(){if(!v){v=!0;do{for(let t=0;t<b.length;t+=1){const e=b[t];g(e),M(e.$$)}for(g(null),b.length=0;w.length;)w.pop()();for(let t=0;t<y.length;t+=1){const e=y[t];C.has(e)||(C.add(e),e())}y.length=0}while(b.length);for(;_.length;)_.pop()();A=!1,v=!1,C.clear()}}function M(t){if(null!==t.fragment){t.update(),o(t.before_update);const e=t.dirty;t.dirty=[-1],t.fragment&&t.fragment.p(t.ctx,e),t.after_update.forEach(R)}}const E=new Set;function S(t,e){t&&t.i&&(E.delete(t),t.i(e))}function U(t,e,n,o){if(t&&t.o){if(E.has(t))return;E.add(t),undefined.c.push((()=>{E.delete(t),o&&(n&&t.d(1),o())})),t.o(e)}}function P(t){t&&t.c()}function H(t,n,l,c){const{fragment:s,on_mount:i,on_destroy:u,after_update:f}=t.$$;s&&s.m(n,l),c||R((()=>{const n=i.map(e).filter(r);u?u.push(...n):o(n),t.$$.on_mount=[]})),f.forEach(R)}function O(t,e){const n=t.$$;null!==n.fragment&&(o(n.on_destroy),n.fragment&&n.fragment.d(e),n.on_destroy=n.fragment=null,n.ctx=[])}function j(t,e){-1===t.$$.dirty[0]&&(b.push(t),A||(A=!0,x.then(k)),t.$$.dirty.fill(0)),t.$$.dirty[e/31|0]|=1<<e%31}function z(e,r,l,c,s,i,f,a=[-1]){const p=h;g(e);const $=e.$$={fragment:null,ctx:null,props:i,update:t,not_equal:s,bound:n(),on_mount:[],on_destroy:[],on_disconnect:[],before_update:[],after_update:[],context:new Map(p?p.$$.context:r.context||[]),callbacks:n(),dirty:a,skip_bound:!1,root:r.target||p.$$.root};f&&f($.root);let d=!1;if($.ctx=l?l(e,r.props||{},((t,n,...o)=>{const r=o.length?o[0]:n;return $.ctx&&s($.ctx[t],$.ctx[t]=r)&&(!$.skip_bound&&$.bound[t]&&$.bound[t](r),d&&j(e,t)),n})):[],$.update(),d=!0,o($.before_update),$.fragment=!!c&&c($.ctx),r.target){if(r.hydrate){const t=function(t){return Array.from(t.childNodes)}(r.target);$.fragment&&$.fragment.l(t),t.forEach(u)}else $.fragment&&$.fragment.c();r.intro&&S(e.$$.fragment),H(e,r.target,r.anchor,r.customElement),k()}g(p)}class F{$destroy(){O(this,1),this.$destroy=t}$on(t,e){const n=this.$$.callbacks[t]||(this.$$.callbacks[t]=[]);return n.push(e),()=>{const t=n.indexOf(e);-1!==t&&n.splice(t,1)}}$set(t){var e;this.$$set&&(e=t,0!==Object.keys(e).length)&&(this.$$.skip_bound=!0,this.$$set(t),this.$$.skip_bound=!1)}}const N=[];function B(e,n=t){let o;const r=new Set;function c(t){if(l(e,t)&&(e=t,o)){const t=!N.length;for(const t of r)t[1](),N.push(t,e);if(t){for(let t=0;t<N.length;t+=2)N[t][0](N[t+1]);N.length=0}}}return{set:c,update:function(t){c(t(e))},subscribe:function(l,s=t){const i=[l,s];return r.add(i),1===r.size&&(o=n(c)||t),l(e),()=>{r.delete(i),0===r.size&&(o(),o=null)}}}}const D=(new signalR.HubConnectionBuilder).withUrl("http://localhost:5000/mutatefulHub").withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol).build();D.start().then((()=>{console.log("Connection established")}),(()=>{console.log("Failed to connect :(")}));const I=t=>({subscribe:B(new Uint8Array,(e=>(D.on("SetClipDataOnClient",((n,o)=>{n===t&&(console.log("DebugMessage - received data: ",o),e(o))})),()=>{console.log("readableClip end called")}))).subscribe});class T{constructor(t){this.notes=[],this.length=t}}class V{constructor(t,e,n,o){this.pitch=t,this.start=e,this.duration=n,this.velocity=o}}function q(t){let e=G(t,2),n=new T(e),o=function(t,e=0){let n=new Uint8Array(2);return n[0]=t[e],n[1]=t[e+1],new Uint16Array(n.buffer)[0]}(t,7),r=9;for(let e=0;e<o;e++)n.notes.push(new V(t[r],G(t,r+1),G(t,r+5),G(t,r+9))),r+=25;return n}function G(t,e=0){let n=new Uint8Array(4);for(let o=0;o<4;o++)n[o]=t[o+e];return new Float32Array(n.buffer)[0]}function J(e){let n,o,r,l,c,h,g,m,b,w=e[0].toUpperCase()+"",y=e[6](e[4])+"";return{c(){n=p(),o=f("div"),r=f("div"),l=f("span"),c=a(w),h=a("Clip title goes here"),g=p(),m=f("canvas"),b=a(y),$(l,"class","clip-slot--ref"),$(r,"class","clip-slot--title"),$(m,"class","clip-slot--preview svelte-1cb6r93"),$(m,"width",e[2]),$(m,"height",e[3]),$(o,"class","clip-slot svelte-1cb6r93")},m(t,u){i(t,n,u),i(t,o,u),s(o,r),s(r,l),s(l,c),s(r,h),s(o,g),s(o,m),s(m,b),e[7](m)},p(t,[e]){1&e&&w!==(w=t[0].toUpperCase()+"")&&d(c,w),16&e&&y!==(y=t[6](t[4])+"")&&d(b,y),4&e&&$(m,"width",t[2]),8&e&&$(m,"height",t[3])},i:t,o:t,d(t){t&&u(n),t&&u(o),e[7](null)}}}function K(t,e,n){let o,{clipRef:r}=e;const l=I(r||"A1");let s;c(t,l,(t=>n(4,o=t)));let i=!1,u=300,f=150;m((()=>{i=!0;let t=getComputedStyle(s);n(2,u=parseInt(t.getPropertyValue("width"),10)),n(3,f=parseInt(t.getPropertyValue("height"),10))}));return t.$$set=t=>{"clipRef"in t&&n(0,r=t.clipRef)},[r,s,u,f,o,l,t=>(i&&(t=>{const e=s.getContext("2d");console.log(u,f),e.clearRect(0,0,u,f),e.fillStyle="white",e.fillRect(0,0,u,f),e.fillStyle="red";let n=Math.min(t.length,16),o=t.notes.map((t=>t.pitch)),r=Math.max(...o),l=Math.min(...o),c=u/n,i=f/(r+1-l);for(let o of t.notes){if(o.start>=n)return;e.fillRect(Math.floor(c*o.start),Math.floor(i*(o.pitch-l)),Math.floor(c),Math.floor(i))}})(q(t)),""),function(t){w[t?"unshift":"push"]((()=>{s=t,n(1,s)}))}]}class L extends F{constructor(t){super(),z(this,t,K,J,l,{clipRef:0})}}function Q(e){let n,o,r,l,c,a,d,h,g,m,b,w,y,_,x,A,R,v,C,k;return r=new L({props:{clipRef:"A1"}}),c=new L({props:{clipRef:"A2"}}),d=new L({props:{clipRef:"A3"}}),g=new L({props:{clipRef:"A4"}}),b=new L({props:{clipRef:"A5"}}),y=new L({props:{clipRef:"A6"}}),x=new L({props:{clipRef:"A7"}}),R=new L({props:{clipRef:"A8"}}),C=new L({props:{clipRef:"A9"}}),{c(){n=p(),o=f("div"),P(r.$$.fragment),l=p(),P(c.$$.fragment),a=p(),P(d.$$.fragment),h=p(),P(g.$$.fragment),m=p(),P(b.$$.fragment),w=p(),P(y.$$.fragment),_=p(),P(x.$$.fragment),A=p(),P(R.$$.fragment),v=p(),P(C.$$.fragment),$(o,"class","cells svelte-t4533h")},m(t,e){i(t,n,e),i(t,o,e),H(r,o,null),s(o,l),H(c,o,null),s(o,a),H(d,o,null),s(o,h),H(g,o,null),s(o,m),H(b,o,null),s(o,w),H(y,o,null),s(o,_),H(x,o,null),s(o,A),H(R,o,null),s(o,v),H(C,o,null),k=!0},p:t,i(t){k||(S(r.$$.fragment,t),S(c.$$.fragment,t),S(d.$$.fragment,t),S(g.$$.fragment,t),S(b.$$.fragment,t),S(y.$$.fragment,t),S(x.$$.fragment,t),S(R.$$.fragment,t),S(C.$$.fragment,t),k=!0)},o(t){U(r.$$.fragment,t),U(c.$$.fragment,t),U(d.$$.fragment,t),U(g.$$.fragment,t),U(b.$$.fragment,t),U(y.$$.fragment,t),U(x.$$.fragment,t),U(R.$$.fragment,t),U(C.$$.fragment,t),k=!1},d(t){t&&u(n),t&&u(o),O(r),O(c),O(d),O(g),O(b),O(y),O(x),O(R),O(C)}}}class W extends F{constructor(t){super(),z(this,t,null,Q,l,{})}}function X(e){let n,o;return n=new W({}),{c(){P(n.$$.fragment)},m(t,e){H(n,t,e),o=!0},p:t,i(t){o||(S(n.$$.fragment,t),o=!0)},o(t){U(n.$$.fragment,t),o=!1},d(t){O(n,t)}}}return new class extends F{constructor(t){super(),z(this,t,null,X,l,{})}}({target:document.body,props:{name:"world"}})}();
//# sourceMappingURL=index.js.map