extends EditorDebuggerPlugin

const LogDebuggerPanel = preload("res://addons/GodotSharpLog/Editor/LogDebuggerPanel.gd");
var panelRes:PackedScene = preload("res://addons/GodotSharpLog/Editor/LogDebuggerPanel.tscn");

var panels:Dictionary[int,LogDebuggerPanel] = {};
var sessions:Array[EditorDebuggerSession] = [];
func _has_capture(capture: String) -> bool:
	return capture == "gd_log";
	
func _capture(message: String, data: Array, session_id: int) -> bool:
	if message.ends_with(":message"):
		if self.panels.has(session_id):
			var i := 0;
			var panel = self.panels[session_id];
			while i < data.size():
				panel.add_log(data[i],data[i+1],data[i+2],data[i+3]);
				i += 4;
		return true;
	elif message.ends_with(":category"):
		if self.panels.has(session_id):
			var i := 0;
			var panel = self.panels[session_id];
			while i < data.size():
				panel.add_category(data[i]);
				i += 1;
		return true;
	return false;
	
func _setup_session(session_id: int) -> void:
	var session: EditorDebuggerSession = get_session(session_id);	
	var panel: LogDebuggerPanel = panelRes.instantiate();
	session.add_session_tab(panel);
	self.panels[session_id] = panel;
	panel.init(session_id,session);
	session.started.connect(panel.start);
	self.sessions.push_back(session)

func stop_all_profiler():
	for session in self.sessions:
		session.toggle_profiler("gd_log",false)
