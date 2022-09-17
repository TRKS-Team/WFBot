// https://blog.expo.io/building-a-code-editor-with-monaco-f84b3a06deaf



// ReSharper disable StringLiteralWrongQuotes

let editor;
let model;

function set_editor_content(s) {
    require.config({
        paths: {
            'vs': 'https://cdn.jsdelivr.net/npm/monaco-editor@0.20.0/min/vs'
        }
    });
    require(['vs/editor/editor.main'], function () {
        
        model = monaco.editor.createModel(s, "csharp");
        editor.setModel(model);
    });
}

function get_editor_content() {
    return model.getValue();
}

function load_editor() {
    require.config({
        paths: {
            'vs': 'https://cdn.jsdelivr.net/npm/monaco-editor@0.20.0/min/vs'
        }
    });
    

    let currentFile = '/index.php';



    // localStorage.removeItem('files');

    
    require(['vs/editor/editor.main'], function () {

        monaco.languages.typescript.javascriptDefaults.setEagerModelSync(true);
        monaco.languages.typescript.javascriptDefaults.setCompilerOptions({
            allowNonTsExtensions: true
        });


        editor = monaco.editor.create(document.getElementById('container'), {
            automaticLayout: true,
            scrollBeyondLastLine: false,
            model: null,
            readOnly: false,
            theme: "vs-dark",
            // roundedSelection: false,
        });
        


    });
}

