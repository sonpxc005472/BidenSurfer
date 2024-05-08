ARG NODE_IMAGE=node:16.20.2

FROM ${NODE_IMAGE} as node-build
WORKDIR /app

COPY BidenSurfer.WebPage/Web/package.json .
COPY BidenSurfer.WebPage/Web/craco.config.js .
RUN yarn install && yarn build && ls -la /app

COPY BidenSurfer.WebPage/Web/ .

FROM node-build as publish

FROM nginx:perl as final
WORKDIR /app

RUN nginx -v
EXPOSE 80

COPY --from=node-build /app/nginx/nginx.conf /etc/nginx/nginx.conf
COPY --from=node-build /app/nginx/default.conf /etc/nginx/conf.d/default.conf
COPY --from=node-build /app/build /app

CMD ["nginx", "-g", "daemon off;"]