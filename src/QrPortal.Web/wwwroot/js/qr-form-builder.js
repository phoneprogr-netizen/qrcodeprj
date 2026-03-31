window.renderQrPayloadForm = function (schema, containerId) {
  const container = document.getElementById(containerId);
  container.innerHTML = '';
  if (!schema || !schema.properties) {
    container.innerHTML = '<textarea name="PayloadJson" class="form-control" rows="8">{}</textarea>';
    return;
  }
  Object.keys(schema.properties).forEach(key => {
    const item = schema.properties[key];
    const wrap = document.createElement('div');
    wrap.className = 'mb-2';
    wrap.innerHTML = `<label class="form-label">${key}</label><input class="form-control" data-field="${key}" placeholder="${item.description ?? ''}"/>`;
    container.appendChild(wrap);
  });
};
