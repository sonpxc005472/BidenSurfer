"use strict";(self.webpackChunkbiden_web=self.webpackChunkbiden_web||[]).push([[81],{8081:(e,s,r)=>{r.r(s),r.d(s,{default:()=>A});var n=r(5043),t=r(9092),i=r(6576),a=r(5958),d=r(4075),o=r(579);const l=()=>{const{t:e}=(0,t.B)();return(0,o.jsx)(a.P.Item,{name:"confirmPassword",label:e("profile.nav.securitySettings.confirmPassword"),dependencies:["newPassword"],rules:[{required:!0,message:e("profile.nav.securitySettings.requiredPassword")},s=>{let{getFieldValue:r}=s;return{validator:(s,n)=>n&&r("newPassword")!==n?Promise.reject(new Error(e("profile.nav.securitySettings.dontMatchPassword"))):Promise.resolve()}}],children:(0,o.jsx)(d.c,{})})},c=()=>{const{t:e}=(0,t.B)();return(0,o.jsx)(a.P.Item,{name:"password",label:e("profile.nav.securitySettings.currentPassword"),rules:[{required:!0,message:e("profile.nav.securitySettings.requiredPassword")}],children:(0,o.jsx)(d.c,{})})},u=new RegExp(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$/),m=()=>{const{t:e}=(0,t.B)();return(0,o.jsx)(a.P.Item,{name:"newPassword",label:e("profile.nav.securitySettings.newPassword"),dependencies:["password"],rules:[{required:!0,message:e("profile.nav.securitySettings.requiredPassword")},{pattern:u,message:e("profile.nav.securitySettings.notValidPassword")},s=>{let{getFieldValue:r}=s;return{validator:(s,n)=>n&&r("password")===n?Promise.reject(new Error(e("profile.nav.securitySettings.samePassword"))):Promise.resolve()}}],children:(0,o.jsx)(d.c,{})})};var x,w=r(9018),h=r(7528),g=r(4684),j=r(5953);const p=(0,r(4574).Ay)(g.S)(x||(x=(0,h.A)(["\n  margin: 0.5rem 0 1.5rem 0;\n  width: 100%;\n\n  @media only screen and "," {\n    max-width: 10rem;\n  }\n\n  @media only screen and "," {\n    max-width: unset;\n  }\n"])),j.$_.md,j.$_.xl);var P=r(9800),v=r(1645);const f=()=>{const[e,s]=(0,n.useState)(!1),[r,i]=(0,n.useState)(!1),{t:d}=(0,t.B)();return(0,o.jsx)(a.P,{name:"newPassword",requiredMark:"optional",isFieldsChanged:e,onFieldsChange:()=>s(!0),footer:(0,o.jsx)(p,{loading:r,type:"primary",htmlType:"submit",children:d("common.confirm")}),onFinish:e=>{i(!0),setTimeout((()=>{i(!1),s(!1),w.O.success({message:d("common.success")}),console.log(e)}),1e3)},children:(0,o.jsxs)(P.A,{gutter:{md:15,xl:30},children:[(0,o.jsx)(v.A,{span:24,children:(0,o.jsx)(a.P.Item,{children:(0,o.jsx)(a.P.Title,{children:d("profile.nav.securitySettings.changePassword")})})}),(0,o.jsx)(v.A,{xs:24,md:12,xl:24,children:(0,o.jsx)(c,{})}),(0,o.jsx)(v.A,{xs:24,md:12,xl:24,children:(0,o.jsx)(m,{})}),(0,o.jsx)(v.A,{xs:24,md:12,xl:24,children:(0,o.jsx)(l,{})})]})})},y=()=>(0,o.jsx)(i.a,{children:(0,o.jsx)(P.A,{gutter:[30,0],children:(0,o.jsx)(v.A,{xs:24,xl:24,children:(0,o.jsx)(f,{})})})});var S=r(8670);const A=()=>{const{t:e}=(0,t.B)();return(0,o.jsxs)(o.Fragment,{children:[(0,o.jsx)(S.s,{children:e("profile.nav.securitySettings.title")}),(0,o.jsx)(y,{})]})}}}]);
//# sourceMappingURL=81.c17c5c5b.chunk.js.map