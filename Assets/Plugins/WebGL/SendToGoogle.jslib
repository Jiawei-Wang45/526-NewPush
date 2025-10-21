mergeInto(LibraryManager.library, {
  SubmitFormJS: function(urlPtr, keysJsonPtr, valsJsonPtr) {
    var url = UTF8ToString(urlPtr);
    var keysJson = UTF8ToString(keysJsonPtr);
    var valsJson = UTF8ToString(valsJsonPtr);

    try {
      var keys = JSON.parse(keysJson).items;
      var vals = JSON.parse(valsJson).items;

      // create hidden iframe to target the form submission so the page won't navigate
      var iframe = document.createElement('iframe');
      iframe.style.display = 'none';
      // give it a unique name
      var iframeName = 'submit_iframe_' + Date.now();
      iframe.name = iframeName;
      document.body.appendChild(iframe);

      var form = document.createElement('form');
      form.style.display = 'none';
      form.method = 'POST';
      form.action = url;
      form.target = iframeName;

      for (var i = 0; i < keys.length; i++) {
        var input = document.createElement('input');
        input.type = 'hidden';
        input.name = keys[i];
        input.value = vals[i];
        form.appendChild(input);
      }

      document.body.appendChild(form);
      form.submit();

      // cleanup after a short delay
      setTimeout(function() {
        try { document.body.removeChild(form); } catch(e){}
        try { document.body.removeChild(iframe); } catch(e){}
      }, 2000);
    } catch (e) {
      console.error('SubmitFormJS error:', e);
    }
  }
});
