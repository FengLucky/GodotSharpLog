extends EditorDebuggerPlugin

const LogPanel = preload("res://addons/GodotSharpLog/Core/LogPanel.gd");
var panelRes:PackedScene = preload("res://addons/GodotSharpLog/Resources/LogPanel.tscn");

var panels:Dictionary[int,LogPanel] = {};
var sessions:Array[EditorDebuggerSession] = [];
func _has_capture(capture: String) -> bool:
	return capture == "gd_log";
	
func _capture(message: String, data: Array, session_id: int) -> bool:
	if message.ends_with(":message"):
		if self.panels.has(session_id):
			self.panels[session_id].add_logs(data);
		return true;
	elif message.ends_with(":category"):
		if self.panels.has(session_id):
			self.panels[session_id].add_categories(data);
		return true;
	return false;
	
func _setup_session(session_id: int) -> void:
	var session: EditorDebuggerSession = get_session(session_id);	
	var panel: LogPanel = panelRes.instantiate();
	self.panels[session_id] = panel;
	panel.init(session_id,true,self._on_click_code_link);
	session.add_session_tab(panel);
	session.started.connect(func ():
		panel.clear();
		self.toggle_profiler(session,true);
	);
	self.sessions.push_back(session)
	self.toggle_profiler(session,true);

func stop_all_profiler():
	for session in self.sessions:
		self.toggle_profiler(session,false);
		
func toggle_profiler(session:EditorDebuggerSession,enable:bool):
	session.toggle_profiler("gd_log",enable)

func _on_click_code_link(json):
	var param = JSON.parse_string(json);
	var script: Script = load("res://"+param.path);
	EditorInterface.edit_script(script,param.line);	
