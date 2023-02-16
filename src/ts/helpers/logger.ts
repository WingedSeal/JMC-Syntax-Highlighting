import { IChildLogger, IVSCodeExtLogger } from "@vscode-logging/types";
import { NOOP_LOGGER, configureLogger } from "@vscode-logging/wrapper";
import { ExtensionContext, window } from "vscode";

let loggerImpel: IVSCodeExtLogger = NOOP_LOGGER;

const LOGGING_LEVEL_PROP = "Example_Logging.loggingLevel";
const SOURCE_LOCATION_PROP = "Example_Logging.sourceLocationTracking";

export function getLogger(): IChildLogger {
	return loggerImpel;
}

function setLogger(newLogger: IVSCodeExtLogger): void {
	loggerImpel = newLogger;
}

export async function initLogger(context: ExtensionContext): Promise<void> {
	// By asserting the existence of the properties in the package.json
	// at runtime, we avoid many copy-pasta mistakes...

	const extLogger = configureLogger({
		extName: "JMC LOG",
		logPath: context.logUri.fsPath,
		logOutputChannel: window.createOutputChannel("JMC LOG"),
		// set to `true` if you also want your VSCode extension to log to the console.
		logConsole: true,
		loggingLevelProp: LOGGING_LEVEL_PROP,
		sourceLocationProp: SOURCE_LOCATION_PROP,
		subscriptions: context.subscriptions,
	});

	setLogger(extLogger);
}
